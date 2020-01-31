using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator.Handlers
{
    public class ServiceHandler
    {
        private readonly IKubernetes _client;

        public ServiceHandler(IKubernetes client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }
    
        public async Task<V1Service> Get(HealthCheckResource resource)
        {
            var serviceList = await _client.ListNamespacedServiceAsync(resource.Metadata.NamespaceProperty, labelSelector: $"resourceId={resource.Metadata.Uid}");
            return serviceList.Items.FirstOrDefault();
        }

        public async Task<V1Service> Get(string namespaceProperty, string labelSelector)
        {
            var serviceList = await _client.ListNamespacedServiceAsync(namespaceProperty, labelSelector: labelSelector);
            return serviceList.Items.FirstOrDefault();
        }
        public async Task<V1Service> GetOrCreateAsync(HealthCheckResource resource)
        {
            var service = await Get(resource);
            if (service != null) return service;
            
            try
            {
                var serviceResource = Build(resource);
                service = await _client.CreateNamespacedServiceAsync(serviceResource, resource.Metadata.NamespaceProperty);
                Console.WriteLine($"Service {service.Metadata.Name} has been created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating service for hc resource {resource.Spec.Name}: {ex.Message}");
            }

            return service;
        }

        public async Task Delete(HealthCheckResource resource)
        {
            try {
                await _client.DeleteNamespacedServiceAsync($"{resource.Metadata.Name}-svc", resource.Metadata.NamespaceProperty);
            }
            catch(Exception ex) {
                Console.WriteLine($"Error deleting service for hc resource {resource.Spec.Name}: {ex.Message}");
            }       
        }
        public V1Service Build(HealthCheckResource resource)
        {
            var meta = new V1ObjectMeta
            {
                Name = $"{resource.Metadata.Name}-svc",
                Labels = new Dictionary<string, string>
                {
                    ["resourceId"] = resource.Metadata.Uid,
                    ["app"] = resource.Spec.Name
                },
            };

            var spec = new V1ServiceSpec
            {
                Selector = new Dictionary<string, string>
                {
                    ["app"] = resource.Spec.Name
                },
                Type = "LoadBalancer",
                Ports = new List<V1ServicePort> {
                    new V1ServicePort {
                        Name = "httport",
                        Port = int.Parse(resource.Spec.ListeningPort),
                        TargetPort = 80,
                        
                    }
                }
            };

            return new V1Service(metadata: meta, spec: spec);
        }
        public async Task<bool> Exists(IKubernetes client, HealthCheckResource resource)
        {
            var service = await client.ListNamespacedServiceAsync(resource.Metadata.NamespaceProperty, labelSelector: $"resourceId={resource.Metadata.Uid}");
            return service.Items.Any();
        }
    }
}