using k8s.Models;

#nullable enable
namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal class KubernetesAddressFactory
    {
        private readonly KubernetesDiscoverySettings _settings;

        public KubernetesAddressFactory(KubernetesDiscoverySettings discoveryOptions)
        {
            _settings = discoveryOptions;
        }

        public string CreateAddress(V1Service service)
        {
            string address = string.Empty;

            var port = GetServicePortValue(service);

            switch (service.Spec.Type)
            {
                case ServiceType.LOAD_BALANCER:
                case ServiceType.NODE_PORT:
                    address = GetLoadBalancerAddress(service);
                    break;
                case ServiceType.CLUSTER_IP:
                    address = service.Spec.ClusterIP;
                    break;
                case ServiceType.EXTERNAL_NAME:
                    address = service.Spec.ExternalName;
                    break;
            }

            string healthPath = _settings.HealthPath;
            if (!string.IsNullOrEmpty(_settings.ServicesPathAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesPathAnnotation) ?? false))
            {
                healthPath = service.Metadata.Annotations![_settings.ServicesPathAnnotation]!;
            }
            healthPath = healthPath.TrimStart('/');

            string healthScheme = "http";
            if (!string.IsNullOrEmpty(_settings.ServicesSchemeAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesSchemeAnnotation) ?? false))
            {
                healthScheme = service.Metadata.Annotations![_settings.ServicesSchemeAnnotation]!.ToLower();
            }

            // Support IPv6 address hosts
            return address.Contains(':')
                ? $"{healthScheme}://[{address}]{port}/{healthPath}"
                : $"{healthScheme}://{address}{port}/{healthPath}";
        }

        private static string GetLoadBalancerAddress(V1Service service)
        {
            var firstIngress = service.Status?.LoadBalancer?.Ingress?.FirstOrDefault();
            if (firstIngress is V1LoadBalancerIngress ingress)
            {
                return string.IsNullOrEmpty(ingress.Ip) ? ingress.Hostname : ingress.Ip;
            }

            return service.Spec.ClusterIP;
        }

        private string GetServicePortValue(V1Service service)
        {
            int? port;
            switch (service.Spec.Type)
            {
                case ServiceType.LOAD_BALANCER:
                case ServiceType.CLUSTER_IP:
                    port = GetServicePort(service)?.Port;
                    break;
                case ServiceType.NODE_PORT:
                    port = GetServicePort(service)?.NodePort;
                    break;
                case ServiceType.EXTERNAL_NAME:
                    port = GetServicePortAnnotation(service) is string servicePortAnnotation && int.TryParse(servicePortAnnotation, out var servicePort)
                        ? servicePort
                        : null;
                    break;
                default:
                    port = null;
                    break;
            }

            return port is null ? string.Empty : $":{port.Value}";
        }

        private V1ServicePort? GetServicePort(V1Service service)
        {
            if (GetServicePortAnnotation(service) is string portAnnotationValue)
            {
                if (int.TryParse(portAnnotationValue, out var portAnnotationIntValue))
                {
                    return service.Spec?.Ports?.Where(p => p.Port == portAnnotationIntValue)?.FirstOrDefault();
                }
                else
                {
                    return service.Spec?.Ports?.Where(p => p.Name == portAnnotationValue)?.FirstOrDefault();
                }
            }
            else
            {
                return service.Spec?.Ports?.FirstOrDefault();
            }
        }

        private string? GetServicePortAnnotation(V1Service service)
        {
            if (!string.IsNullOrEmpty(_settings.ServicesPortAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesPortAnnotation) ?? false))
            {
                return service.Metadata.Annotations![_settings.ServicesPortAnnotation]!;
            }
            return null;
        }
    }
}
