using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.UI.Core.Discovery.K8S.Extensions
{
    internal static class KubernetesHttpClientExtensions
    {
        internal static void ConfigureKubernetesClient(this HttpClient client, IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<KubernetesDiscoveryOptions>();
            
            var validHttpEndpoint = Uri.TryCreate(options.ClusterHost, UriKind.Absolute, out var host)
                && (host.Scheme == Uri.UriSchemeHttp || host.Scheme == Uri.UriSchemeHttps);

            if (!validHttpEndpoint)
            {
                throw new Exception($"{nameof(host)} is not a valid Http Uri");
            }

            client.BaseAddress = host;
                    
             if (!string.IsNullOrEmpty(options.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
            }
        }

        internal static async Task<ServiceList> GetServices(this HttpClient client, string label = "")
        {
            var response = await client.GetAsync($"{client.BaseAddress.AbsoluteUri}{KubernetesApiEndpoints.ServicesV1}?labelSelector={label}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ServiceList>(content);
        }
    }
}
