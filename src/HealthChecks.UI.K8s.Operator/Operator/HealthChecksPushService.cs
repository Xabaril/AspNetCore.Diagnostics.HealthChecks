using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Operator;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator
{
    public class HealthChecksPushService
    {
        public static async Task PushNotification(
            WatchEventType eventType,
            HealthCheckResource resource,
            V1Service uiService,
            V1Service notificationService,
            V1Secret endpointSecret,
            ILogger<K8sOperator> logger)
        {
            var address = KubernetesAddressFactory.CreateHealthAddress(notificationService, resource);
            var uiAddress = KubernetesAddressFactory.CreateAddress(uiService, resource);

            dynamic healthCheck = new
            {
                Type = eventType,
                notificationService.Metadata.Name,
                Uri = address
            };

            using var client = new HttpClient();
            try
            {
                string type = healthCheck.Type.ToString();
                string name = healthCheck.Name;
                string uri = healthCheck.Uri;

                logger.LogInformation("[PushService] Sending Type: {type} - Service {name} with uri : {uri} to ui endpoint: {address}", type, name, uri, uiAddress);

                var key = Encoding.UTF8.GetString(endpointSecret.Data["key"]);

                var response = await client.PostAsync($"{uiAddress}{Constants.PushServicePath}?{Constants.PushServiceAuthKey}={key}",

                  new StringContent(JsonSerializer.Serialize(healthCheck, new JsonSerializerOptions
                  {
                      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                  }), Encoding.UTF8, "application/json"));


                logger.LogInformation("[PushService] Notification result for {name} - status code: {statuscode}", notificationService.Metadata.Name, response.StatusCode);

            }
            catch (Exception ex)
            {
                logger.LogError("Error notifying healthcheck service: {message}", ex.Message);
            }
        }

        private static (string address, V1ServicePort port) GetServiceAddress(V1Service service)
        {
            string IpAddress = default;

            if (service.Spec.Type == ServiceType.LoadBalancer)
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