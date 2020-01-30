using k8s;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Controller;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthChecksOperator : IKubernetesOperator
    {
        private Watcher<HealthCheckResource> _watcher;
        private readonly IKubernetes _client;
        private readonly IHealthChecksController _controller;
        private readonly HealthCheckServiceWatcher _serviceWatcher;
        private readonly string _namespace;

        public HealthChecksOperator(
            IKubernetes client,
            IHealthChecksController controller,
            HealthCheckServiceWatcher serviceWatcher)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _serviceWatcher = serviceWatcher;
        }

        private async Task StartWatcher()
        {
            var response = await _client.ListClusterCustomObjectWithHttpMessagesAsync(
                group: Constants.Group,
                version: Constants.Version,
                plural: Constants.Plural,
                watch: true,
                timeoutSeconds: ((int)TimeSpan.FromMinutes(60).TotalSeconds)
                );

            _watcher = response.Watch<HealthCheckResource, object>(
                onEvent: async (type, item) => await OnEventHandlerAsync(type, item)
                ,
                onClosed: () =>
                {
                    _watcher.Dispose();
                    StartWatcher();
                },
                onError: e => Console.WriteLine(e.Message)
                );
        }
        

        public async Task RunAsync(CancellationToken token)
        {
            await StartWatcher();
            await token.WaitAsync();
        }

        private async Task OnEventHandlerAsync(WatchEventType type, HealthCheckResource item)
        {
            if (type == WatchEventType.Added)
            {
                await _controller.DeployAsync(item);
                await _serviceWatcher.WatchAsync(item);
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
