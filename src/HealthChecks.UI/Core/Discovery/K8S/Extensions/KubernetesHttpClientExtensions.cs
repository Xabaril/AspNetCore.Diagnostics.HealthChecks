using Newtonsoft.Json;
using System;
using System.IO;
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
            var options = serviceProvider.GetRequiredService<KubernetesDiscoverySettings>();
            
            string token;
            string hostString;

            if(options.InCluster)
            {
                var clusterHost = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
                var clusterPort = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");

                string fullClusterHost;
                // Support IPv6 address hosts
                if(clusterHost.Contains(":"))
                {
                    hostString = $"https://[{clusterHost}]:{clusterPort}";
                }
                else
                {
                    hostString = $"https://{clusterHost}:{clusterPort}";
                }

                token = File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token").Trim();
            }
            else
            {
                token = options.Token;
                hostString = options.ClusterHost;
            }
            
            var validHttpEndpoint = Uri.TryCreate(hostString, UriKind.Absolute, out var host)
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
