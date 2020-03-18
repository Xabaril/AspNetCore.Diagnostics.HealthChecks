using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthCheckServiceWatcher : IDisposable
    {
        private readonly IKubernetes _client;
        private readonly ILogger<K8sOperator> _logger;
        private Watcher<V1Service> _watcher;
        private Dictionary<HealthCheckResource, Watcher<V1Service>> _watchers = new Dictionary<HealthCheckResource, Watcher<V1Service>>();

        public HealthCheckServiceWatcher(IKubernetes client, ILogger<K8sOperator> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal Watcher<V1Service> Watch(HealthCheckResource resource, CancellationToken token)
        {
            Func<HealthCheckResource, bool> filter = (k) => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;

            if (!_watchers.Keys.Any(filter))
            {
                var response = _client.ListNamespacedServiceWithHttpMessagesAsync(
                    namespaceParameter: resource.Metadata.NamespaceProperty,
                    labelSelector: $"{resource.Spec.ServicesLabel}",
                    watch: true,
                    cancellationToken: token);

                _watcher = response.Watch<V1Service, V1ServiceList>(
                    onEvent: async (type, item) => await OnServiceDiscoveredAsync(type, item, resource),
                    onError: e => _logger.LogError(e, "Error in service watcher: {message}", e.Message)
                );

                _watchers.Add(resource, _watcher);

                return _watcher;
            }

            return null;
        }

        internal void Stopwatch(HealthCheckResource resource)
        {
            Func<HealthCheckResource, bool> filter = (k) => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;
            if (_watchers.Keys.Any(filter))
            {
                var svcResource = _watchers.Keys.FirstOrDefault(filter);
                if (svcResource != null)
                {
                    _logger.LogInformation("Stopping services watcher for namespace {namespace}", resource.Metadata.NamespaceProperty);
                    _watchers[svcResource].Dispose();
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
                _logger);
        }

        public void Dispose()
        {
            _watchers.Values.ToList().ForEach(w => w?.Dispose());
        }
    }
}