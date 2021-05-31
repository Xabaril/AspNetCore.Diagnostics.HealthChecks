using k8s.Models;
using System;
using System.Linq;

namespace HealthChecks.UI.K8s.Operator.Operator
{
    class KubernetesAddressFactory
    {
        public static string CreateAddress(V1Service service, HealthCheckResource resource)
        {
            string address = string.Empty;

            var port = GetServicePortValue(service) ?? Constants.DefaultPort;

            switch (service.Spec.Type)
            {
                case ServiceType.LoadBalancer:
                case ServiceType.NodePort:
                    address = GetLoadBalancerAddress(service);
                    break;
                case ServiceType.ClusterIP:
                    address = service.Spec.ClusterIP;
                    break;
                case ServiceType.ExternalName:
                    address = service.Spec.ExternalName;
                    break;
            }

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
                return $"{healthScheme}://[{address}]{port}";
            }
            else
            {
                return $"{healthScheme}://{address}{port}";
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

        private static string GetServicePortValue(V1Service service)
        {
            int? port;
            switch (service.Spec.Type)
            {
                case ServiceType.LoadBalancer:
                case ServiceType.ClusterIP:
                case ServiceType.ExternalName:
                    port = GetServicePort(service)?.Port;
                    break;
                case ServiceType.NodePort:
                    port = GetServicePort(service)?.NodePort;
                    break;
                default:
                    port = null;
                    break;
            }

            return port is null ? string.Empty : $":{port.Value}";
        }
        private static V1ServicePort GetServicePort(V1Service service)
        {
            return service.Spec.Ports.FirstOrDefault();
        }
    }
}
