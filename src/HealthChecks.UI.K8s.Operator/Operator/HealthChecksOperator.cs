using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Operator;
using k8s;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static HealthChecks.UI.K8s.Operator.Constants;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthChecksOperator : IHostedService
    {
        private Watcher<HealthCheckResource> _watcher;
        private readonly IKubernetes _client;
        private readonly IHealthChecksController _controller;
        private readonly NamespacedServiceWatcher _serviceWatcher;
        private readonly ClusterServiceWatcher _clusterServiceWatcher;
        private readonly OperatorDiagnostics _diagnostics;
        private readonly ILogger<K8sOperator> _logger;
        private readonly CancellationTokenSource _operatorCts = new CancellationTokenSource();
        private readonly Channel<ResourceWatch> _channel;
        private const int WAIT_FOR_REPLICA_DELAY = 5000;
        private const int WAIT_FOR_REPLICA_RETRIES = 10;

        public HealthChecksOperator(
            IKubernetes client,
            IHealthChecksController controller,
            NamespacedServiceWatcher serviceWatcher,
            ClusterServiceWatcher clusterServiceWatcher,
            OperatorDiagnostics diagnostics,
            ILogger<K8sOperator> logger)
        {

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _serviceWatcher = serviceWatcher ?? throw new ArgumentNullException(nameof(serviceWatcher));
            _clusterServiceWatcher = clusterServiceWatcher ?? throw new ArgumentNullException(nameof(clusterServiceWatcher));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _channel = Channel.CreateUnbounded<ResourceWatch>(new UnboundedChannelOptions
            {
                SingleWriter = true,
                SingleReader = true
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _diagnostics.OperatorStarting();

            _ = Task.Run(OperatorListenerAsync);
            await StartWatcherAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _diagnostics.OperatorShuttingDown();
            _operatorCts.Cancel();

            if (_watcher != null && _watcher.Watching)
            {
                _watcher.Dispose();
            }

            _serviceWatcher.Dispose();
            _clusterServiceWatcher.Dispose();
            _channel.Writer.Complete();

            return Task.CompletedTask;
        }

        private async Task StartWatcherAsync(CancellationToken token)
        {
            var response = await _client.ListClusterCustomObjectWithHttpMessagesAsync(
                group: Constants.GROUP,
                version: Constants.VERSION,
                plural: Constants.PLURAL,
                watch: true,
                timeoutSeconds: ((int)TimeSpan.FromMinutes(60).TotalSeconds),
                cancellationToken: token
                );

            _watcher = response.Watch<HealthCheckResource, object>(
                onEvent: async (eventType, item) =>
                {
                    await _channel.Writer.WriteAsync(new ResourceWatch
                    {
                        EventType = eventType,
                        Resource = item
                    }, token);
                }
                ,
                onClosed: () =>
                {
                    _watcher.Dispose();
                    _ = StartWatcherAsync(token);
                },
                onError: e => _logger.LogError(e.Message)
                );
        }

        private async Task OperatorListenerAsync()
        {
            while (await _channel.Reader.WaitToReadAsync() && !_operatorCts.IsCancellationRequested)
            {
                while (_channel.Reader.TryRead(out ResourceWatch item))
                {
                    try
                    {
                        if (item.EventType == WatchEventType.Added)
                        {
                            _ = Task.Run(async () =>
                            {
                                await _controller.DeployAsync(item.Resource);
                                await WaitForAvailableReplicasAsync(item.Resource);
                                await StartServiceWatcherAsync(item.Resource);
                            });
                        }
                        else if (item.EventType == WatchEventType.Deleted)
                        {
                            await _controller.DeleteDeploymentAsync(item.Resource);
                            StopServiceWatcher(item.Resource);
                        }
                    }
                    catch (Exception ex)
                    {
                        _diagnostics.OperatorThrow(ex);
                    }
                }
            }
        }

        private async Task StartServiceWatcherAsync(HealthCheckResource resource)
        {
            Func<Task> startWatcher = async () => await _serviceWatcher.Watch(resource, _operatorCts.Token);
            Func<Task> startClusterWatcher = async () => await _clusterServiceWatcher.Watch(resource, _operatorCts.Token);

            var start = resource.Spec.Scope switch
            {
                Deployment.Scope.NAMESPACED => startWatcher,
                Deployment.Scope.CLUSTER => startClusterWatcher,
                _ => throw new ArgumentOutOfRangeException(nameof(resource.Spec.Scope))
            };

            await start();
        }

        private void StopServiceWatcher(HealthCheckResource resource)
        {
            Action stopWatcher = () => _serviceWatcher.Stopwatch(resource);
            Action stopClusterWatcher = () => _clusterServiceWatcher.Stopwatch(resource);

            var stop = resource.Spec.Scope switch
            {
                Deployment.Scope.NAMESPACED => stopWatcher,
                Deployment.Scope.CLUSTER => stopClusterWatcher,
                _ => throw new ArgumentOutOfRangeException(nameof(resource.Spec.Scope))
            };

            stop();
        }

        private async Task WaitForAvailableReplicasAsync(HealthCheckResource resource)
        {
            int retries = 1;
            int availableReplicas = 0;

            while (retries <= WAIT_FOR_REPLICA_RETRIES && availableReplicas == 0)
            {
                var deployment = await _client.ListNamespacedOwnedDeploymentAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);

                availableReplicas = deployment.Status.AvailableReplicas ?? 0;

                if (availableReplicas == 0)
                {
                    _logger.LogInformation("The UI replica {Name} in {Namespace} is not available yet, retrying...{Retries}/{MaxRetries}", deployment.Metadata.Name, resource.Metadata.NamespaceProperty, retries, WAIT_FOR_REPLICA_RETRIES);
                    await Task.Delay(WAIT_FOR_REPLICA_DELAY);
                    retries++;
                }
            }
        }

        private class ResourceWatch
        {
            public WatchEventType EventType { get; set; }
            public HealthCheckResource Resource { get; set; }
        }
    }
}
