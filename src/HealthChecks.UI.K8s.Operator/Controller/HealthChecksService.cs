using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    public class HealthChecksService
    {

        public static V1Service Create(HealthCheckResource resource)
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
        public static async Task<bool> Exists(IKubernetes client, HealthCheckResource resource)
        {
            var service = await client.ListNamespacedServiceAsync(resource.Metadata.NamespaceProperty, labelSelector: $"resourceId={resource.Metadata.Uid}");
            return service.Items.Any();
        }
    }
}