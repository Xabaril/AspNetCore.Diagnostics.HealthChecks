using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Operator;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator
{
    public class HealthChecksPushService
    {
        public static async Task PushNotification(
            WatchEventType eventType,
            HealthCheckResource resource,
            V1Service uiService,
            V1Service notificationService,
            V1Secret endpointSecret)
        {
            var address = KubernetesAddressFactory.CreateHealthAddress(notificationService);
            var uiAddress = KubernetesAddressFactory.CreateAddress(uiService);

            dynamic healthCheck = new
            {
                Type = eventType,
                notificationService.Metadata.Name,
                Uri = address
            };

            using var client = new HttpClient();
            try
            {
                Console.WriteLine($"[PushService] Sending Type: {healthCheck.Type} - Service {notificationService.Metadata.Name} with uri : {healthCheck.Uri} to ui endpoint: {uiAddress}");

                var key = Encoding.UTF8.GetString(endpointSecret.Data["key"]);

                var response = await client.PostAsync($"{uiAddress}{Constants.PushServicePath}?{Constants.PushServiceAuthKey}={key}",

                  new StringContent(JsonSerializer.Serialize(healthCheck, new JsonSerializerOptions
                  {
                      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                  }), Encoding.UTF8, "application/json"));


                Console.WriteLine($"[PushService] Notification result for {notificationService.Metadata.Name} - status code: {response.StatusCode}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying healthcheck service: {ex.Message}");
            }
        }

        private static (string address, V1ServicePort port) GetServiceAddress(V1Service service)
        {

            string IpAddress = default;

            if (service.Spec.Type == PortType.LoadBalancer)
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