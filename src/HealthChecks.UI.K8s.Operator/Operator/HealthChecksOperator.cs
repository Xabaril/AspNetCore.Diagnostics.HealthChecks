using HealthChecks.UI.K8s.Operator.Controller;
using k8s;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthChecksOperator : IKubernetesOperator
    {
        private Watcher<HealthCheckResource> _watcher;
        private readonly IKubernetes _client;
        private readonly IHealthChecksController _controller;
        private readonly HealthCheckServiceWatcher _serviceWatcher;
        private readonly ILogger<K8sOperator> _logger;

        public HealthChecksOperator(
            IKubernetes client,
            IHealthChecksController controller,
            HealthCheckServiceWatcher serviceWatcher,
            ILogger<K8sOperator> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _serviceWatcher = serviceWatcher;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                onEvent: async (type, item) => await OnEventHandlerAsync(type, item, token)
                ,
                onClosed: () =>
                {
                    _watcher.Dispose();
                    _ = StartWatcher(token);
                },
                onError: e => _logger.LogError(e.Message)
                );
        }


        public async Task RunAsync(CancellationToken token)
        {
            await StartWatcher(token);
        }

        private async Task OnEventHandlerAsync(WatchEventType type, HealthCheckResource item, CancellationToken token)
        {
            if (type == WatchEventType.Added)
            {
                await _controller.DeployAsync(item);
                _serviceWatcher.Watch(item, token);
            }

            if (type == WatchEventType.Deleted)
            {
                await _controller.DeleteDeploymentAsync(item);
                _serviceWatcher.Stopwatch(item);
            }
        }

        public void Dispose()
        {
            _serviceWatcher?.Dispose();
            _watcher?.Dispose();
        }
    }
}
