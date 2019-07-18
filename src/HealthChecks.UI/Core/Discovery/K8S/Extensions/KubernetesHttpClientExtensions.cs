using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

                // Support IPv6 address hosts
                if(clusterHost.Contains(":"))
                {
                    hostString = $"https://[{clusterHost}]:{clusterPort}";
                }
                else
                {
                    hostString = $"https://{clusterHost}:{clusterPort}";
                }

                var tokenPath = "/var/run/secrets/kubernetes.io/serviceaccount/token";
                if(!File.Exists(tokenPath)) {
                    throw new Exception($"A Kubernetes Service Account token was not mounted");
                }
                token = File.ReadAllText(tokenPath).Trim();
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
                throw new Exception($"{hostString} is not a valid Http Uri");
            }

            client.BaseAddress = host;
                    
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        internal static async Task<ServiceList> GetServices(this HttpClient client, ILogger logger, string label = "", string[] k8sNamespaces = null)
        {
            if(k8sNamespaces == null || k8sNamespaces.Length <= 1)
            {
                return await client.GetServices(logger, label, k8sNamespaces?.FirstOrDefault() ?? "");
            }
            else
            {
                var responses = await Task.WhenAll(k8sNamespaces.Select(k8sNamespace => client.GetServices(logger, label, k8sNamespace)));

                return new ServiceList {
                    Items = responses.SelectMany(r => r.Items).ToArray()
                };
            }
        }

        private static async Task<ServiceList> GetServices(this HttpClient client, ILogger logger, string label, string k8sNamespace)
        {
            string apiPath;
            if(string.IsNullOrEmpty(k8sNamespace))
            {
                apiPath = KubernetesApiEndpoints.ServicesV1;
            }
            else
            {
                apiPath = string.Format(KubernetesApiEndpoints.NamespacedServicesV1, Uri.EscapeDataString(k8sNamespace));
            }
            var response = await client.GetAsync($"{client.BaseAddress.AbsoluteUri}{apiPath}?labelSelector={label}");
            if(!response.IsSuccessStatusCode)
            {
                logger.LogWarning($"Received HTTP {response.StatusCode} {response.ReasonPhrase} when making Kubernetes Service Discovery request to {apiPath}");
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ServiceList>(content);
        }
    }
}
