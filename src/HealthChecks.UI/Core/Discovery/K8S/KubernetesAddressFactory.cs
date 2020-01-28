using System.Linq;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal class KubernetesAddressFactory
    {
        private readonly KubernetesDiscoverySettings _settings;
        public KubernetesAddressFactory(KubernetesDiscoverySettings discoveryOptions)
        {
            _settings = discoveryOptions;
        }
        public string CreateAddress(Service service)
        {
            string address = string.Empty;

            var port = GetServicePortValue(service);

            switch (service.Spec.PortType)
            {
                case PortType.LoadBalancer:
                case PortType.NodePort:
                    address = GetLoadBalancerAddress(service);
                    break;
                case PortType.ClusterIP:
                    address = service.Spec.ClusterIP;
                    break;
            }

            string healthPath;
            if(!string.IsNullOrEmpty(_settings.ServicesPathLabel) && (service.Metadata.Labels?.ContainsKey(_settings.ServicesPathLabel) ?? false))
            {
                healthPath = service.Metadata.Labels[_settings.ServicesPathLabel];
            }
            else if(!string.IsNullOrEmpty(_settings.ServicesPathAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesPathAnnotation) ?? false))
            {
                healthPath = service.Metadata.Annotations[_settings.ServicesPathAnnotation];
            }
            else
            {
                healthPath = _settings.HealthPath;
            }
            healthPath = healthPath.TrimStart('/');

            string healthScheme;
            if(!string.IsNullOrEmpty(_settings.ServicesSchemeLabel) && (service.Metadata.Labels?.ContainsKey(_settings.ServicesSchemeLabel) ?? false))
            {
                healthScheme = service.Metadata.Labels[_settings.ServicesSchemeLabel].ToLower();
            }
            else if(!string.IsNullOrEmpty(_settings.ServicesSchemeAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesSchemeAnnotation) ?? false))
            {
                healthScheme = service.Metadata.Annotations[_settings.ServicesSchemeAnnotation].ToLower();
            }
            else
            {
                healthScheme = "http";
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
        private string GetLoadBalancerAddress(Service service)
        {
            var ingress = service.Status?.LoadBalancer?.Ingress?.FirstOrDefault();
            if (ingress != null)
            {
                return ingress.Ip ?? ingress.HostName;
            }

            return service.Spec.ClusterIP;
        }
        private string GetServicePortValue(Service service)
        {
            string port = string.Empty;

            switch (service.Spec.PortType)
            {
                case PortType.LoadBalancer:
                case PortType.ClusterIP:
                    port = GetServicePort(service)?.PortNumber.ToString() ?? "";
                    break;
                case (PortType.NodePort):
                    port = GetServicePort(service)?.NodePort.ToString() ?? "";
                    break;
            }

            if (!string.IsNullOrEmpty(port))
            {
                port = $":{port}";
            }
            return port;
        }
        private Port GetServicePort(Service service)
        {
            if(!string.IsNullOrEmpty(_settings.ServicesPortLabel) && (service.Metadata.Labels?.ContainsKey(_settings.ServicesPortLabel) ?? false))
            {
                var labelValue = service.Metadata.Labels[_settings.ServicesPortLabel];
                if(int.TryParse(labelValue, out var labelIntValue)) {
                    return service.Spec?.Ports?.Where(p => p.PortNumber == labelIntValue)?.FirstOrDefault();
                } else {
                    return service.Spec?.Ports?.Where(p => p.Name == labelValue)?.FirstOrDefault();
                }
            }
            else if(!string.IsNullOrEmpty(_settings.ServicesPortAnnotation) && (service.Metadata.Annotations?.ContainsKey(_settings.ServicesPortAnnotation) ?? false))
            {
                var annotationValue = service.Metadata.Annotations[_settings.ServicesPortAnnotation];
                if(int.TryParse(annotationValue, out var annotationIntValue)) {
                    return service.Spec?.Ports?.Where(p => p.PortNumber == annotationIntValue)?.FirstOrDefault();
                } else {
                    return service.Spec?.Ports?.Where(p => p.Name == annotationValue)?.FirstOrDefault();
                }
            }
            else
            {
                return service.Spec?.Ports?.FirstOrDefault();
            }
        }
    }
}
