using k8s.Models;
using System.Linq;

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

            string healthPath = _settings.HealthPath;
            if(!string.IsNullOrEmpty(_settings.ServicesPathAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesPathAnnotation) ?? false))
            {
                healthPath = service.Metadata.Annotations![_settings.ServicesPathAnnotation]!;
            }
            healthPath = healthPath.TrimStart('/');

            string healthScheme = "http";
            if(!string.IsNullOrEmpty(_settings.ServicesSchemeAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesSchemeAnnotation) ?? false))
            {
                healthScheme = service.Metadata.Annotations![_settings.ServicesSchemeAnnotation]!.ToLower();
            }

            // Support IPv6 address hosts
            if(address.Contains(":"))
            {
                return $"{healthScheme}://[{address}]{port}/{healthPath}";
            }
            else
            {
                return $"{healthScheme}://{address}{port}/{healthPath}";
            }
        }
        private string GetLoadBalancerAddress(V1Service service)
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
                case ServiceType.LoadBalancer:
                case ServiceType.ClusterIP:
                    port = GetServicePort(service)?.Port;
                    break;
                case ServiceType.NodePort:
                    port = GetServicePort(service)?.NodePort;
                    break;
                case ServiceType.ExternalName:
                    if(GetServicePortAnnotation(service) is string servicePortAnnotation && int.TryParse(servicePortAnnotation, out var servicePort))
                    {
                        port = servicePort;
                    }
                    else
                    {
                        port = null;
                    }
                    break;
                default:
                    port = null;
                    break;
            }

            return port is null ? string.Empty : $":{port.Value}";
        }
        private V1ServicePort? GetServicePort(V1Service service)
        {
            if(GetServicePortAnnotation(service) is string portAnnotationValue)
            {
                if(int.TryParse(portAnnotationValue, out var portAnnotationIntValue)) {
                    return service.Spec?.Ports?.Where(p => p.Port == portAnnotationIntValue)?.FirstOrDefault();
                } else {
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
            if(!string.IsNullOrEmpty(_settings.ServicesPortAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesPortAnnotation) ?? false))
            {
                return service.Metadata.Annotations![_settings.ServicesPortAnnotation]!;
            }
            return null;
        }
    }
}
