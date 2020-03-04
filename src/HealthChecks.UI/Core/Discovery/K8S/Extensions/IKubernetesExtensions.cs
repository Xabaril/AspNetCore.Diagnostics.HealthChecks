using k8s;
using k8s.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace HealthChecks.UI.Core.Discovery.K8S.Extensions
{
    internal static class KubernetesHttpClientExtensions
    {
        internal static async Task<V1ServiceList> GetServices(this IKubernetes client, string label, IEnumerable<string>? k8sNamespaces, CancellationToken cancellationToken)
        {
            if(k8sNamespaces is null || !k8sNamespaces.Any())
            {
                return await client.GetServices(label, k8sNamespaces?.FirstOrDefault(), cancellationToken);
            }
            else
            {
                var responses = await Task.WhenAll(k8sNamespaces.Select(k8sNamespace => client.GetServices(label, k8sNamespace, cancellationToken)));
                
                return new V1ServiceList()
                {
                    Items = responses.SelectMany(r => r.Items).ToList()
                };
            }
        }

        private static async Task<V1ServiceList> GetServices(this IKubernetes client, string label, string? k8sNamespace, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(k8sNamespace))
            {
                return await client.ListServiceForAllNamespacesAsync(labelSelector: label, cancellationToken: cancellationToken);
            }
            else
            {
                return await client.ListNamespacedServiceAsync(k8sNamespace, labelSelector: label, cancellationToken: cancellationToken);
            }
        }
    }
}
