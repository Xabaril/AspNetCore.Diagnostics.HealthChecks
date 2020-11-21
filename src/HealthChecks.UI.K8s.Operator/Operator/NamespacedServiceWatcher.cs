using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator
{
    internal class NamespacedServiceWatcher : IDisposable
    {
        private readonly IKubernetes _client;
        private readonly ILogger<K8sOperator> _logger;
        private readonly OperatorDiagnostics _diagnostics;
        private readonly NotificationHandler _notificationHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private Dictionary<HealthCheckResource, Watcher<V1Service>> _watchers = new Dictionary<HealthCheckResource, Watcher<V1Service>>();

        public NamespacedServiceWatcher(
            IKubernetes client,
            ILogger<K8sOperator> logger,
            OperatorDiagnostics diagnostics,
            NotificationHandler notificationHandler,
            IHttpClientFactory httpClientFactory)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _notificationHandler = notificationHandler ?? throw new ArgumentNullException(nameof(notificationHandler));
        }

        internal Task Watch(HealthCheckResource resource, CancellationToken token)
        {
            Func<HealthCheckResource, bool> filter = (k) => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;

            if (!_watchers.Keys.Any(filter))
            {
                var response = _client.ListNamespacedServiceWithHttpMessagesAsync(
                    namespaceParameter: resource.Metadata.NamespaceProperty,
                    labelSelector: $"{resource.Spec.ServicesLabel}",
                    watch: true,
                    cancellationToken: token);

                var watcher = response.Watch<V1Service, V1ServiceList>(
                    onEvent: async (type, item) => await _notificationHandler.NotifyDiscoveredServiceAsync(type, item, resource),
                    onError: e =>
                    {
                        _diagnostics.ServiceWatcherThrow(e);
                        Watch(resource, token);
                    }
                );

                _diagnostics.ServiceWatcherStarting(resource.Metadata.NamespaceProperty);

                _watchers.Add(resource, watcher);
            }

            return Task.CompletedTask;
        }

        internal void Stopwatch(HealthCheckResource resource)
        {
            Func<HealthCheckResource, bool> filter = (k) => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;
            if (_watchers.Keys.Any(filter))
            {
                var svcResource = _watchers.Keys.FirstOrDefault(filter);
                if (svcResource != null)
                {
                    _diagnostics.ServiceWatcherStopped(resource.Metadata.NamespaceProperty);
                    _watchers[svcResource]?.Dispose();
                    _watchers.Remove(svcResource);
                }
            }
        }

        internal async Task OnServiceDiscoveredAsync(WatchEventType type, V1Service service, HealthCheckResource resource)
        {
            var uiService = await _client.ListNamespacedOwnedServiceAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
            var secret = await _client.ListNamespacedOwnedSecretAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);

            if (!service.Metadata.Labels.ContainsKey(resource.Spec.ServicesLabel))
            {
                type = WatchEventType.Deleted;
            }

            await HealthChecksPushService.PushNotification(
                type,
                resource,
                uiService,
                service,
                secret,
                _logger,
                _httpClientFactory);
        }

        public void Dispose()
        {
            _watchers.Values.ToList().ForEach(w =>
            {
                if (w != null && w.Watching) w.Dispose();
            });
        }
    }
}