using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator
{
    internal class HealthCheckServiceWatcher
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
            if (!_watchers.ContainsKey(resource))
            {
                
                var response =  _client.ListNamespacedServiceWithHttpMessagesAsync(
                    namespaceParameter: resource.Metadata.NamespaceProperty, 
                    labelSelector: $"{resource.Spec.HealthchecksLabel}=true",
                    watch: true);
    
                _watcher = response.Watch<V1Service, V1ServiceList>(
                    onEvent: async (type, item) => await OnServiceDiscoveredAsync(type, item, resource),
                    onClosed: () =>
                    {
                        Stopwatch(resource);
                        WatchAsync(resource);
                    },
                    onError: e => Console.WriteLine(e.Message)
                );
            
                _watchers.Add(resource, _watcher);
            
                return _watcher;
            }

            return null;
        }

        internal void Stopwatch(HealthCheckResource resource)
        {
            if (_watchers.ContainsKey(resource))
            {
                _watchers[resource].Dispose();
                _watchers.Remove(resource);
            }
        }

         internal Task OnServiceDiscoveredAsync(WatchEventType type, V1Service item, HealthCheckResource resource) {
             return Task.CompletedTask;
         }
    }

}