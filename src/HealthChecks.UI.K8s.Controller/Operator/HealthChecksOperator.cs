using HealthChecks.UI.K8s.Controller.Controller;
using k8s;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Controller
{
    internal class HealthChecksOperator : IKubernetesOperator
    {
        private Watcher<HealthCheckResource> _watcher;
        private readonly Kubernetes _client;
        private readonly IHealthChecksController _controller;
        private readonly string _namespace;

        public HealthChecksOperator(
            Kubernetes client,
            IHealthChecksController controller,
            string @namespace)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _namespace = @namespace ?? throw new ArgumentNullException(nameof(client));
        }

        private async Task StartWatcher()
        {
            var response = await _client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                group: Constants.Group,
                version: Constants.Version,
                namespaceParameter: _namespace,
                plural: Constants.Plural,
                watch: true,
                timeoutSeconds: ((int)TimeSpan.FromMinutes(60).TotalSeconds)
                );

            _watcher = response.Watch<HealthCheckResource, object>(
                onEvent: async(type, item) => await OnEventHandlerAsync(type, item)
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

        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
