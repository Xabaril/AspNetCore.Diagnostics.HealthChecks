using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator.Operator
{
    internal class ClusterServiceWatcher
    {
        private readonly IKubernetes _client;
        private readonly ILogger<K8sOperator> _logger;
        private readonly OperatorDiagnostics _diagnostics;
        private readonly NotificationHandler _notificationHandler;
        private Watcher<V1Service> _watcher;
        public ClusterServiceWatcher(
          IKubernetes client,
          ILogger<K8sOperator> logger,
          OperatorDiagnostics diagnostics,
          NotificationHandler notificationHandler
          )
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _notificationHandler = notificationHandler ?? throw new ArgumentNullException(nameof(notificationHandler));
        }

        internal Task Watch(HealthCheckResource resource, CancellationToken token)
        {
            var response = _client.ListServiceForAllNamespacesWithHttpMessagesAsync(
                labelSelector: $"{resource.Spec.ServicesLabel}",
                watch: true,
                cancellationToken: token);

            _watcher = response.Watch<V1Service, V1ServiceList>(
                onEvent: async (type, item) => await _notificationHandler.NotifyDiscoveredServiceAsync(type, item, resource),
                onError: e =>
                {
                    _diagnostics.ServiceWatcherThrow(e);
                    Watch(resource, token);
                }
            );

            _diagnostics.ServiceWatcherStarting("All");

            return Task.CompletedTask;
        }

        internal void Stopwatch(HealthCheckResource resource)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_watcher != null && _watcher.Watching)
            {
                _watcher.Dispose();
            }
        }
    }
}
