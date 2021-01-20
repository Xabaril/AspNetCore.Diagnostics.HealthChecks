using k8s.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kubernetes
{
    public class KubernetesChecksExecutor
    {
        private readonly k8s.Kubernetes _client;
        private readonly Dictionary<Type, Func<KubernetesResourceCheck, CancellationToken, Task<(bool, string)>>> _handlers;
        public KubernetesChecksExecutor(k8s.Kubernetes client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _handlers = new Dictionary<Type, Func<KubernetesResourceCheck, CancellationToken, Task<(bool, string)>>>()
            {
                [typeof(V1Deployment)] = CheckDeploymentAsync,
                [typeof(V1Service)] = CheckServiceAsync,
                [typeof(V1Pod)] = CheckPodAsync
            };
        }
        public Task<(bool, string)> CheckAsync(KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
        {
            var handler = _handlers[resourceCheck.ResourceType];
            return handler?.Invoke(resourceCheck, cancellationToken) ??
                   throw new InvalidOperationException(
                       $"No handler registered for type {resourceCheck.ResourceType.Name}");
        }
        private async Task<(bool, string)> CheckDeploymentAsync(KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
        {
            var tsc = new TaskCompletionSource<(bool, string)>();

            try
            {
                var result = await _client.ReadNamespacedDeploymentStatusWithHttpMessagesAsync(resourceCheck.Name,
                    resourceCheck.Namespace, cancellationToken: cancellationToken);

                tsc.SetResult((resourceCheck.Check(result.Body), resourceCheck.Name));
            }
            catch (Exception ex)
            {
                tsc.SetException(
                    new Exception($"The Deployment {resourceCheck.Name} failed with error: {ex.Message}"));
            }

            return await tsc.Task;
        }
        private async Task<(bool, string)> CheckPodAsync(KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
        {
            var tsc = new TaskCompletionSource<(bool, string)>();
            try
            {
                var result = await _client.ReadNamespacedPodStatusWithHttpMessagesAsync(resourceCheck.Name,
                    resourceCheck.Namespace, cancellationToken: cancellationToken);

                tsc.SetResult((resourceCheck.Check(result.Body), resourceCheck.Name));
            }
            catch (Exception ex)
            {
                tsc.SetException(
                    new Exception($"The pod {resourceCheck.Name} failed with error: {ex.Message}"));
            }

            return await tsc.Task;
        }
        private async Task<(bool, string)> CheckServiceAsync(KubernetesResourceCheck resourceCheck, CancellationToken cancellationToken)
        {
            var tsc = new TaskCompletionSource<(bool, string)>();
            try
            {
                var result = await _client.ReadNamespacedServiceStatusWithHttpMessagesAsync(resourceCheck.Name,
                    resourceCheck.Namespace, cancellationToken: cancellationToken);

                tsc.SetResult((resourceCheck.Check(result.Body), resourceCheck.Name));
            }
            catch (Exception ex)
            {
                tsc.SetException(
                    new Exception($"The service {resourceCheck.Name} failed with error: {ex.Message}"));
            }

            return await tsc.Task;
        }
    }
}