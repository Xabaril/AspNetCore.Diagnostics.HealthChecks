using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Extensions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Handlers
{
    public class ServiceHandler
    {
        private readonly IKubernetes _client;
        private readonly ILogger<K8sOperator> _logger;

        public ServiceHandler(IKubernetes client, ILogger<K8sOperator> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<V1Service> Get(HealthCheckResource resource)
        {
            return _client.ListNamespacedOwnedServiceAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
        }

        public async Task<V1Service> GetOrCreateAsync(HealthCheckResource resource)
        {
            var service = await Get(resource);
            if (service != null) return service;

            try
            {
                var serviceResource = Build(resource);
                service = await _client.CreateNamespacedServiceAsync(serviceResource, resource.Metadata.NamespaceProperty);
                _logger.LogInformation("Service {name} has been created", service.Metadata.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating service for hc resource {name} : {message}", resource.Spec.Name, ex.Message);
            }

            return service;
        }

        public async Task Delete(HealthCheckResource resource)
        {
            try
            {
                await _client.DeleteNamespacedServiceAsync($"{resource.Spec.Name}-svc", resource.Metadata.NamespaceProperty);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting service for hc resource {name} : {message}", resource.Spec.Name, ex.Message);
            }
        }
        public V1Service Build(HealthCheckResource resource)
        {
            var meta = new V1ObjectMeta
            {
                Name = $"{resource.Spec.Name}-svc",
                OwnerReferences = new List<V1OwnerReference> {
                    resource.CreateOwnerReference()
                },
                Annotations = new Dictionary<string, string>(),
                Labels = new Dictionary<string, string>
                {
                    ["app"] = resource.Spec.Name
                },
            };

            var spec = new V1ServiceSpec
            {
                Selector = new Dictionary<string, string>
                {
                    ["app"] = resource.Spec.Name
                },
                Type = resource.Spec.ServiceType ?? Constants.DefaultServiceType,
                Ports = new List<V1ServicePort> {
                    new V1ServicePort {
                        Name = "httport",
                        Port = int.Parse(resource.Spec.PortNumber ?? Constants.DefaultPort),
                        TargetPort = 80
                    }
                }
            };

            foreach (var annotation in resource.Spec.ServiceAnnotations)
            {
                _logger.LogInformation("Adding annotation {Annotation} to ui service with value {AnnotationValue}", annotation.Name, annotation.Value);
                meta.Annotations.Add(annotation.Name, annotation.Value);
            }

            return new V1Service(metadata: meta, spec: spec);
        }
    }
}