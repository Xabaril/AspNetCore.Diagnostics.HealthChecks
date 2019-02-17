using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.Kubernetes
{
    public class KubernetesChecksExecutor
    {
        private readonly k8s.Kubernetes _client;

        private Dictionary<Type, Func<KubernetesResourceCheck, Task<(bool, string)>>> _handlers;


        public KubernetesChecksExecutor(k8s.Kubernetes client)
        {
            _client = client;
            _handlers = new Dictionary<Type, Func<KubernetesResourceCheck, Task<(bool,string)>>>()
            {
                [typeof(V1Deployment)] = CheckDeploymentAsync,
                [typeof(V1Service)] = CheckServiceAsync,
                [typeof(V1Pod)] = CheckPodAsync
            };
        }

        public Task<(bool, string)> CheckAsync(KubernetesResourceCheck resourceCheck)
        {
            var handler = _handlers[resourceCheck.ResourceType];
            return handler?.Invoke(resourceCheck) ??
                   throw new InvalidOperationException(
                       $"No handler registered for type {resourceCheck.ResourceType.Name}");
        }

        private async Task<(bool, string)> CheckDeploymentAsync(KubernetesResourceCheck resourceCheck)
        {
            var result = await _client.ReadNamespacedDeploymentStatusWithHttpMessagesAsync(resourceCheck.Name,
                resourceCheck.Namespace);

            return (resourceCheck.Check(result.Body), resourceCheck.Name);
        }

        private async  Task<(bool, string)> CheckPodAsync(KubernetesResourceCheck resourceCheck)
        {
            var result = await _client.ReadNamespacedPodStatusWithHttpMessagesAsync(resourceCheck.Name,
                resourceCheck.Namespace);

            return (resourceCheck.Check(result.Body), resourceCheck.Name);
        }

        private async  Task<(bool, string)> CheckServiceAsync(KubernetesResourceCheck resourceCheck)
        {
            var result = await _client.ReadNamespacedServiceStatusWithHttpMessagesAsync(resourceCheck.Name,
                resourceCheck.Namespace);

            return (resourceCheck.Check(result.Body), resourceCheck.Name);
        }
    }
}