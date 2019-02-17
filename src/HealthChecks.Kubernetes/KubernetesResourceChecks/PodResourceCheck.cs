using System;
using System.Threading.Tasks;
using k8s.Models;

namespace HealthChecks.Kubernetes.KubernetesResourceChecks
{
    internal class PodResourceCheck: IKubernetesCheck<V1Pod>
    {
        private readonly k8s.Kubernetes _client;

        public PodResourceCheck(k8s.Kubernetes client)
        {
            _client = client;
        }
        public async Task<bool> CheckAsync(KubernetesResource resource, Func<V1Pod, bool> condition)
        {
            var result = await _client.ReadNamespacedPodStatusWithHttpMessagesAsync(resource.Name, resource.Namespace);
            return condition(result.Body);
        }
    }
}