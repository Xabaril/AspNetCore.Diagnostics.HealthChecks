using k8s.Models;

namespace HealthChecks.Kubernetes;

internal static class KubernetesChecksExecutor
{
    private static readonly Dictionary<Type, Func<k8s.Kubernetes, KubernetesResourceCheck, CancellationToken, Task<(bool, string)>>> _handlers = new()
    {
        [typeof(V1Deployment)] = CheckDeploymentAsync,
        [typeof(V1Service)] = CheckServiceAsync,
        [typeof(V1Pod)] = CheckPodAsync
    };

    public static Task<(bool, string)> CheckAsync(k8s.Kubernetes client, KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
    {
        var handler = _handlers[resourceCheck.ResourceType];
        return handler?.Invoke(client, resourceCheck, cancellationToken) ??
               throw new InvalidOperationException(
                   $"No handler registered for type {resourceCheck.ResourceType.Name}");
    }

    private static async Task<(bool, string)> CheckDeploymentAsync(k8s.Kubernetes client, KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
    {
        var tsc = new TaskCompletionSource<(bool, string)>();

        try
        {
            using var result = await client.AppsV1.ReadNamespacedDeploymentStatusWithHttpMessagesAsync(resourceCheck.Name,
                resourceCheck.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false);

            tsc.SetResult((resourceCheck.Check(result.Body), resourceCheck.Name));
        }
        catch (Exception ex)
        {
            tsc.SetException(
                new Exception($"The Deployment {resourceCheck.Name} failed with error: {ex.Message}"));
        }

        return await tsc.Task.ConfigureAwait(false);
    }

    private static async Task<(bool, string)> CheckPodAsync(k8s.Kubernetes client, KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
    {
        var tsc = new TaskCompletionSource<(bool, string)>();
        try
        {
            using var result = await client.CoreV1.ReadNamespacedPodStatusWithHttpMessagesAsync(resourceCheck.Name,
                resourceCheck.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false);

            tsc.SetResult((resourceCheck.Check(result.Body), resourceCheck.Name));
        }
        catch (Exception ex)
        {
            tsc.SetException(
                new Exception($"The pod {resourceCheck.Name} failed with error: {ex.Message}"));
        }

        return await tsc.Task.ConfigureAwait(false);
    }

    private static async Task<(bool, string)> CheckServiceAsync(k8s.Kubernetes client, KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
    {
        var tsc = new TaskCompletionSource<(bool, string)>();
        try
        {
            using var result = await client.CoreV1.ReadNamespacedServiceStatusWithHttpMessagesAsync(resourceCheck.Name,
                resourceCheck.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false);

            tsc.SetResult((resourceCheck.Check(result.Body), resourceCheck.Name));
        }
        catch (Exception ex)
        {
            tsc.SetException(
                new Exception($"The service {resourceCheck.Name} failed with error: {ex.Message}"));
        }

        return await tsc.Task.ConfigureAwait(false);
    }
}
