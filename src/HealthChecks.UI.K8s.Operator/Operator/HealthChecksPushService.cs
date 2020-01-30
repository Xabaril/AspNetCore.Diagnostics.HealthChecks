using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator
{
    public class HealthChecksPushService
    {
        public static async Task PushNotification(WatchEventType eventType, HealthCheckResource resource, V1Service service)
        {
            var (addr, port) = GetServiceAddress(service);
            if (!string.IsNullOrEmpty(addr) && port != null)
            {
                dynamic healthCheck = new
                {
                    Type = eventType,
                    Name = resource.Spec.Name,
                    Uri = $"http://{addr}:{port.Port}"
                };
                
                using var client = new HttpClient();
                await client.PostAsync($"http://localhost:5000/{Constants.PushServicePath}",
                    new StringContent(healthCheck, Encoding.UTF8, "application/json"));
            }
        }

        private static (string address, V1ServicePort port) GetServiceAddress(V1Service service)
        {
            
            string IpAddress = default;
            
            if (service.Spec.Type == "LoadBalancer")
            {
                var ingress = service.Status?.LoadBalancer?.Ingress?.FirstOrDefault();
                if (ingress != null)
                {
                    IpAddress = ingress.Ip ?? ingress.Hostname;
                }
                else
                {
                    IpAddress = service.Spec.ClusterIP;
                }
            }
            else
            {
                IpAddress = service.Spec.ClusterIP;
            }

            return (IpAddress, service.Spec.Ports.FirstOrDefault());
        }
    }
}