using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthCheckServiceWatcher: IDisposable
    {
        private readonly IKubernetes _client;
        private Watcher<V1Service> _watcher;
        private Dictionary<HealthCheckResource, Watcher<V1Service>> _watchers = new Dictionary<HealthCheckResource, Watcher<V1Service>>();

        public HealthCheckServiceWatcher(IKubernetes client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        internal async Task<Watcher<V1Service>> WatchAsync(HealthCheckResource resource)
        {            
            Func<HealthCheckResource, bool> filter = (k) => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;
            
            if (!_watchers.Keys.Any(filter))
            {
                var response =  _client.ListNamespacedServiceWithHttpMessagesAsync(
                    namespaceParameter: resource.Metadata.NamespaceProperty, 
                    labelSelector: $"{resource.Spec.ServicesLabel}",
                    watch: true);
    
                _watcher = response.Watch<V1Service, V1ServiceList>(
                    onEvent: async (type, item) => await OnServiceDiscoveredAsync(type, item, resource),
                    onError: e => Console.WriteLine(e.Message)
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
                    Console.WriteLine($"Stopping services watcher for namespace {resource.Metadata.NamespaceProperty}");
                    _watchers[svcResource].Dispose();
                    _watchers.Remove(svcResource);
                }
            }
        }

        internal async Task OnServiceDiscoveredAsync(WatchEventType type, V1Service service, HealthCheckResource resource)
        {
            var uiService = await _client.ListNamespacedServiceAsync(resource.Metadata.NamespaceProperty, labelSelector: $"resourceId={resource.Metadata.Uid}");

            if(!service.Metadata.Labels.ContainsKey(resource.Spec.ServicesLabel))
            {
                type = WatchEventType.Deleted;
            }

            await HealthChecksPushService.PushNotification(type, resource, uiService.Items.First(), service);
        }

        public void Dispose()
        {
            _watchers.Values.ToList().ForEach(w => w?.Dispose());
        }
    }
}