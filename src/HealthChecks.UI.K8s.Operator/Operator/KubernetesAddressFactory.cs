using k8s.Models;
using System;
using System.Linq;

namespace HealthChecks.UI.K8s.Operator.Operator
{
    class KubernetesAddressFactory
    {
        public static string CreateAddress(V1Service service, HealthCheckResource resource)
        {
            var defaultPort = int.Parse(resource.Spec.PortNumber ?? Constants.DefaultPort);
            var port = GetServicePort(service)?.Port ?? defaultPort;
            var address = service.Spec.ClusterIP;

            string healthScheme = resource.Spec.HealthChecksScheme;

            if (service.Metadata.Annotations?.ContainsKey(Constants.HealthCheckSchemeAnnotation) ?? false)
            {
                healthScheme = service.Metadata.Annotations[Constants.HealthCheckSchemeAnnotation];
            }

            if (healthScheme.IsEmpty())
            {
                healthScheme = Constants.DefaultScheme;
            }

            if (address.Contains(":"))
            {
                return $"{healthScheme}://[{address}]:{port}";
            }
            else
            {
                return $"{healthScheme}://{address}:{port}";
            }
        }

        public static string CreateHealthAddress(V1Service service, HealthCheckResource resource)
        {
            var address = CreateAddress(service, resource);

            string healthPath = resource.Spec.HealthChecksPath;

            if (service.Metadata.Annotations?.ContainsKey(Constants.HealthCheckPathAnnotation) ?? false)
            {
                healthPath = service.Metadata.Annotations[Constants.HealthCheckPathAnnotation];
            }

            if (healthPath.IsEmpty())
            {
                healthPath = Constants.DefaultHealthPath;
            }

            return $"{address}/{ healthPath.TrimStart('/')}";

        }

        private static string GetLoadBalancerAddress(V1Service service)
        {
            var ingress = service.Status.LoadBalancer?.Ingress?.FirstOrDefault();
            if (ingress != null)
            {
                return ingress.Hostname ?? ingress.Ip;
            }

            return service.Spec.ClusterIP;
        }

        private static V1ServicePort GetServicePort(V1Service service)
        {
            return service.Spec.Ports.FirstOrDefault();
        }
    }
}
