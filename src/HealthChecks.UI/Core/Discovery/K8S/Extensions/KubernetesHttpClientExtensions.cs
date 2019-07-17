using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
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
        internal static async Task<ServiceList> GetServices(this HttpClient client, string label = "", string[] k8sNamespaces = null)
        {
            if(k8sNamespaces == null || k8sNamespaces.Length <= 1)
            {
                return await client.GetServices(label, k8sNamespaces?.FirstOrDefault() ?? "");
            }
            else
            {
                var responses = await Task.WhenAll(k8sNamespaces.Select(k8sNamespace => client.GetServices(label, k8sNamespace)));

                return new ServiceList {
                    Items = responses.SelectMany(r => r.Items).ToArray()
                };
            }
        }

        private static async Task<ServiceList> GetServices(this HttpClient client, string label, string k8sNamespace)
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
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ServiceList>(content);
        }
    }
}
