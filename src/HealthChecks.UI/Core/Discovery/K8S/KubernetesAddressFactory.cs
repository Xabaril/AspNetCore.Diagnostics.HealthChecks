using System.Linq;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal class KubernetesAddressFactory
    {
        private readonly string _defaultHealthPath;
        private readonly string _healthPathLabel;
        private readonly string _healthPortLabel;
        private readonly string _healthSchemeLabel;
        public KubernetesAddressFactory(string defaultHealthPath, string healthPathLabel, string healthPortLabel, string healthSchemeLabel)
        {
            _defaultHealthPath = defaultHealthPath;
            _healthPathLabel = healthPathLabel;
            _healthPortLabel = healthPortLabel;
            _healthSchemeLabel = healthSchemeLabel;
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
            if(!string.IsNullOrEmpty(_healthPathLabel) && (service.Metadata.Labels?.ContainsKey(_healthPathLabel) ?? false))
            {
                healthPath = service.Metadata.Labels[_healthPathLabel];
            }
            else
            {
                healthPath = _defaultHealthPath;
            }
            healthPath = healthPath.TrimStart('/');

            string healthScheme;
            if(!string.IsNullOrEmpty(_healthSchemeLabel) && (service.Metadata.Labels?.ContainsKey(_healthSchemeLabel) ?? false))
            {
                healthScheme = service.Metadata.Labels[_healthSchemeLabel].ToLower();
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
            if(!string.IsNullOrEmpty(_healthPortLabel) && (service.Metadata.Labels?.ContainsKey(_healthPortLabel) ?? false))
            {
                var labelValue = service.Metadata.Labels[_healthPortLabel];
                if(int.TryParse(labelValue, out var labelIntValue)) {
                    return service.Spec?.Ports?.Where(p => p.PortNumber == labelIntValue)?.FirstOrDefault();
                } else {
                    return service.Spec?.Ports?.Where(p => p.Name == labelValue)?.FirstOrDefault();
                }
            }
            else
            {
                return service.Spec?.Ports?.FirstOrDefault();
            }
        }
    }
}
