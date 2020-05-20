using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Diagnostics;
using k8s;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthChecksOperator : IHostedService
    {
        private Watcher<HealthCheckResource> _watcher;
        private readonly IKubernetes _client;
        private readonly IHealthChecksController _controller;
        private readonly HealthCheckServiceWatcher _serviceWatcher;
        private readonly OperatorDiagnostics _diagnostics;
        private readonly ILogger<K8sOperator> _logger;
        private readonly CancellationTokenSource _operatorCts = new CancellationTokenSource();
        private readonly Channel<ResourceWatch> _channel;
        private const int WaitForReplicaDelay = 5000;
        private const int WaitForReplicaRetries = 10;

        public HealthChecksOperator(
            IKubernetes client,
            IHealthChecksController controller,
            HealthCheckServiceWatcher serviceWatcher,
            OperatorDiagnostics diagnostics,
            ILogger<K8sOperator> logger)
        {

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _serviceWatcher = serviceWatcher ?? throw new ArgumentNullException(nameof(serviceWatcher));
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

            _ = Task.Run(OperatorListener);
            await StartWatcher(cancellationToken);
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
            _channel.Writer.Complete();

            return Task.CompletedTask;
        }

        private async Task StartWatcher(CancellationToken token)
        {
            var response = await _client.ListClusterCustomObjectWithHttpMessagesAsync(
                group: Constants.Group,
                version: Constants.Version,
                plural: Constants.Plural,
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
                    _ = StartWatcher(token);
                },
                onError: e => _logger.LogError(e.Message)
                );
        }
        private async Task OperatorListener()
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
                                await WaitForAvailableReplicas(item.Resource);
                                await _serviceWatcher.Watch(item.Resource, _operatorCts.Token);
                            });
                        }
                        else if (item.EventType == WatchEventType.Deleted)
                        {
                            await _controller.DeleteDeploymentAsync(item.Resource);
                            _serviceWatcher.Stopwatch(item.Resource);
                        }
                    }
                    catch (Exception ex)
                    {
                        _diagnostics.OperatorThrow(ex);
                    }
                }
            }
        }

        private async Task WaitForAvailableReplicas(HealthCheckResource resource)
        {
            int retries = 1;
            int availableReplicas = 0;

            while (retries <= WaitForReplicaRetries && availableReplicas == 0)
            {
                var deployment = await _client.ListNamespacedOwnedDeploymentAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);

                availableReplicas = deployment.Status.AvailableReplicas ?? 0;

                if (availableReplicas == 0)
                {
                    _logger.LogInformation("The UI replica {Name} in {Namespace} is not available yet, retrying...{Retries}/{MaxRetries}", deployment.Metadata.Name, resource.Metadata.NamespaceProperty, retries, WaitForReplicaRetries);
                    await Task.Delay(WaitForReplicaDelay);
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
