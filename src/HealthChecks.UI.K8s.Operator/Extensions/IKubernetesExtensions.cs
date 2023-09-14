using k8s.Models;

namespace k8s;

public static class IKubernetesExtensions
{
    public static async Task<V1Deployment?> ListNamespacedOwnedDeploymentAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
    {
        var services = await client.AppsV1.ListNamespacedDeploymentAsync(k8sNamespace).ConfigureAwait(false);
        return services.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
    }

    public static async Task<V1Service?> ListNamespacedOwnedServiceAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
    {
        var services = await client.CoreV1.ListNamespacedServiceAsync(k8sNamespace).ConfigureAwait(false);
        return services.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
    }

    public static async Task<V1Secret?> ListNamespacedOwnedSecretAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
    {
        var services = await client.CoreV1.ListNamespacedSecretAsync(k8sNamespace).ConfigureAwait(false);
        return services.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
    }

    public static async Task<V1ConfigMap?> ListNamespacedOwnedConfigMapAsync(this IKubernetes client, string k8sNamespace, string ownerUniqueId)
    {
        var configMaps = await client.CoreV1.ListNamespacedConfigMapAsync(k8sNamespace).ConfigureAwait(false);
        return configMaps.Items.FirstOrDefault(i => i.Metadata.OwnerReferences?.Any(or => or.Uid == ownerUniqueId) ?? false);
    }
}
