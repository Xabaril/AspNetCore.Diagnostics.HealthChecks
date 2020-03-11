using k8s.Models;
using System.Linq;
using System.Threading.Tasks;

namespace k8s
{
    public static class IKubernetesExtensions
    {
        public static async Task<V1Deployment> ListNamespacedOwnedDeploymentAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
        {
            var services = await client.ListNamespacedDeploymentAsync(k8sNamespace);
            return services.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
        }

        public static async Task<V1Service> ListNamespacedOwnedServiceAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
        {
            var services = await client.ListNamespacedServiceAsync(k8sNamespace);
            return services.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
        }

        public static async Task<V1Secret> ListNamespacedOwnedSecretAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
        {
            var services = await client.ListNamespacedSecretAsync(k8sNamespace);
            return services.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
        }
    }
}
