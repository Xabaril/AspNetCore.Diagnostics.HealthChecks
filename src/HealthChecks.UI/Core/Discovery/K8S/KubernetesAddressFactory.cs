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
            var port = GetServicePortValue(service);

            var address = (_settings.UseDNSNames, service.Spec.Type) switch {
                (_, ServiceType.LoadBalancer) => GetLoadBalancerAddress(service),
                (_, ServiceType.NodePort) => GetLoadBalancerAddress(service),
                (_, ServiceType.ExternalName) => service.Spec.ExternalName,
                (true, _) => GetKubeDNSName(service),
                (_, ServiceType.ClusterIP) => service.Spec.ClusterIP,
                (_, _) => service.Spec.ClusterIP,
            };

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

            var portStr = (port, healthScheme) switch {
                var _ when port is null => string.Empty,
                (80, "http") => string.Empty,
                (443, "https") => string.Empty,
                (_, _) => ":" + port.Value
            };

            // Support IPv6 address hosts
            if(address.Contains(":"))
            {
                return $"{healthScheme}://[{address}]{portStr}/{healthPath}";
            }
            else
            {
                return $"{healthScheme}://{address}{portStr}/{healthPath}";
            }
        }
        private string GetKubeDNSName(V1Service service) => service.Metadata.Name + "." + service.Metadata.NamespaceProperty;
        private string GetLoadBalancerAddress(V1Service service)
        {
            var firstIngress = service.Status?.LoadBalancer?.Ingress?.FirstOrDefault();
            if (firstIngress is V1LoadBalancerIngress ingress)
            {
                return string.IsNullOrEmpty(ingress.Ip) ? ingress.Hostname : ingress.Ip;
            }

            return _settings.UseDNSNames ? GetKubeDNSName(service) : service.Spec.ClusterIP;
        }
        private int? GetServicePortValue(V1Service service)
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

            return port;
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
