using System;
using System.Threading.Tasks;
using k8s.Models;

namespace HealthChecks.Kubernetes.KubernetesResourceChecks
{
    internal class ServiceResourceCheck : IKubernetesCheck<V1Service>
    {
        private readonly k8s.Kubernetes _client;

        public ServiceResourceCheck(k8s.Kubernetes client)
        {
            _client = client;
        }

        public async Task<bool> CheckAsync(KubernetesResource resource, Func<V1Service, bool> condition)
        {
            var result =
                await _client.ReadNamespacedServiceStatusWithHttpMessagesAsync(resource.Name, resource.Namespace);
            return condition(result.Body);
        }
    }
}