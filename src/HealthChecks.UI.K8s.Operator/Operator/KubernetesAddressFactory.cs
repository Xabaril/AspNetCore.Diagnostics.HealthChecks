using k8s.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthChecks.UI.K8s.Operator.Operator
{
    class KubernetesAddressFactory
    {
        private static int DefaultPort = 80;
        public static string CreateAddress(V1Service service)
        {

            var port = service.Spec.Type switch
            {
                PortType.LoadBalancer => GetServicePort(service)?.Port ?? DefaultPort,
                PortType.ClusterIP => GetServicePort(service)?.Port ?? DefaultPort,
                PortType.NodePort => GetServicePort(service)?.NodePort ?? DefaultPort
            };

            var address = service.Spec.Type switch
            {
                PortType.LoadBalancer => GetLoadBalancerAddress(service),
                PortType.NodePort => GetLoadBalancerAddress(service),
                PortType.ClusterIP => service.Spec.ClusterIP
            };

            var healthPath = service.Metadata.Labels.ContainsKey(Constants.ServicesHealthPathLabel) ?
                    service.Metadata.Labels[Constants.ServicesHealthPathLabel] :
                    "health";

            var healthScheme = service.Metadata.Labels.ContainsKey(Constants.ServicesSchemeLabel) ?
                    service.Metadata.Labels[Constants.ServicesSchemeLabel] :
                    "http";

            if (address.Contains(":"))
            {
                return $"{healthScheme}://[{address}]{port}/{healthPath}";
            }
            else
            {
                return $"{healthScheme}://{address}{port}/{healthPath}";
            }
        }
        private static string GetLoadBalancerAddress(V1Service service)
        {
            var ingress = service.Status?.LoadBalancer?.Ingress?.FirstOrDefault();
            if (ingress != null)
            {
                return ingress.Ip ?? ingress.Hostname;
            }

            return service.Spec.ClusterIP;
        }

        private static V1ServicePort GetServicePort(V1Service service)
        {
            return service.Spec.Ports.FirstOrDefault();
        }
    }
}
