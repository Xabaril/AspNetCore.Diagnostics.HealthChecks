using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal class KubernetesAddressFactory
    {
        private readonly string _healthPath;

        public KubernetesAddressFactory(string healthPath)
        {
            _healthPath = healthPath;
        }

        public string CreateAddress(Service service)
        {
            string address = string.Empty;

            var port = GetServicePort(service);

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

            return $"http://{address}{port}/{_healthPath}";
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

        private string GetServicePort(Service service)
        {
            string port = string.Empty;

            switch (service.Spec.PortType)
            {
                case PortType.LoadBalancer:
                case PortType.ClusterIP:
                    port = service.Spec?.Ports?.FirstOrDefault()?.PortNumber.ToString() ?? "";
                    break;
                case (PortType.NodePort):
                    port = service.Spec?.Ports?.FirstOrDefault()?.NodePort.ToString() ?? "";
                    break;
            }

            if (!string.IsNullOrEmpty(port))
            {
                port = $":{port}";
            }
            return port;
        }
    }
}
