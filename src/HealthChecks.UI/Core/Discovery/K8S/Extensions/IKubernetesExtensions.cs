using k8s;
using k8s.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace HealthChecks.UI.Core.Discovery.K8S.Extensions
{
    internal static class IKubernetesExtensions
    {
        internal static async Task<IEnumerable<V1Service>> GetServices(this IKubernetes client, string label, IEnumerable<string>? k8sNamespaces, CancellationToken cancellationToken)
        {
            if (k8sNamespaces is null || !k8sNamespaces.Any())
            {
                var services = await client.ListServiceForAllNamespacesAsync(labelSelector: label, cancellationToken: cancellationToken);
                return services?.Items ?? Enumerable.Empty<V1Service>();
            }
            else
            {
                var responses = await Task.WhenAll(k8sNamespaces.Select(k8sNamespace => client.ListNamespacedServiceAsync(k8sNamespace, labelSelector: label, cancellationToken: cancellationToken)));
                
                return responses.Select(s => s?.Items).Where(s => s != null).SelectMany(s => s).ToList();
            }
        }
    }
}
