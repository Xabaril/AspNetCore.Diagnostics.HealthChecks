using System.Text;
using System.Text.Json;
using HealthChecks.UI.K8s.Operator.Operator;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator;

public class HealthChecksPushService
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

#pragma warning disable IDE1006 // Naming Styles
    public static async Task PushNotification( //TODO: rename public API
#pragma warning restore IDE1006 // Naming Styles
        WatchEventType eventType,
        HealthCheckResource resource,
        V1Service uiService,
        V1Service notificationService,
        V1Secret endpointSecret,
        ILogger<K8sOperator> logger,
        IHttpClientFactory httpClientFactory)
    {
        var address = KubernetesAddressFactory.CreateHealthAddress(notificationService, resource);
        var uiAddress = KubernetesAddressFactory.CreateAddress(uiService, resource);

        dynamic healthCheck = new
        {
            Type = eventType,
            notificationService.Metadata.Name,
            Uri = address
        };

        using var client = httpClientFactory.CreateClient();
        try
        {
            string type = healthCheck.Type.ToString();
            string name = healthCheck.Name;
            string uri = healthCheck.Uri;

            logger.LogInformation("[PushService] Namespace {Namespace} - Sending Type: {type} - Service {name} with uri : {uri} to ui endpoint: {address}", resource.Metadata.NamespaceProperty, type, name, uri, uiAddress);

            var key = Encoding.UTF8.GetString(endpointSecret.Data["key"]);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{uiAddress}{Constants.PUSH_SERVICE_PATH}?{Constants.PUSH_SERVICE_AUTH_KEY}={key}")
            {
                Content = new StringContent(JsonSerializer.Serialize(healthCheck, _options), Encoding.UTF8, "application/json")
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            logger.LogInformation("[PushService] Notification result for {name} - status code: {statuscode}", notificationService.Metadata.Name, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError("Error notifying healthcheck service: {message}", ex.Message);
        }
    }

    private static (string address, V1ServicePort? port) GetServiceAddress(V1Service service)
    {
        string IpAddress;

        if (service.Spec.Type == ServiceType.LoadBalancer)
        {
            var ingress = service.Status?.LoadBalancer?.Ingress?.FirstOrDefault();
            IpAddress = ingress == null
                ? service.Spec.ClusterIP
                : ingress.Ip ?? ingress.Hostname;
        }
        else
        {
            IpAddress = service.Spec.ClusterIP;
        }

        return (IpAddress, service.Spec.Ports.FirstOrDefault());
    }
}
