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
        internal static async Task<V1ServiceList> GetServices(this IKubernetes client, string label, List<string> k8sNamespaces, CancellationToken cancellationToken)
        {
            if(k8sNamespaces is null || !k8sNamespaces.Any())
            {
                return await client.ListServiceForAllNamespacesAsync(labelSelector: label, cancellationToken: cancellationToken);
            }
            else
            {
                var responses = await Task.WhenAll(k8sNamespaces.Select(k8sNamespace => client.ListNamespacedServiceAsync(k8sNamespace, labelSelector: label, cancellationToken: cancellationToken)));
                
                return new V1ServiceList()
                {
                    Items = responses.SelectMany(r => r.Items).ToList()
                };
            }
        }
    }
}
