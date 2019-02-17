using System;
using System.Threading.Tasks;
using k8s.Models;

namespace HealthChecks.Kubernetes.KubernetesResourceChecks
{
    internal class DeploymentResourceCheck: IKubernetesCheck<V1Deployment>
    {
        private readonly k8s.Kubernetes _client;

        public DeploymentResourceCheck(k8s.Kubernetes client)
        {
            _client = client;
        }
        
        public async Task<bool> CheckAsync(KubernetesResource resource, Func<V1Deployment, bool> condition)
        {
            var result = await _client.ReadNamespacedDeploymentStatusWithHttpMessagesAsync(resource.Name, resource.Namespace);
            return condition(result.Body);
        }
    }
}