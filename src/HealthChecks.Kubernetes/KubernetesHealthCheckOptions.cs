using System;
using System.Collections.Concurrent;
using k8s;
using k8s.Models;

namespace HealthChecks.Kubernetes
{
    
    public class KubernetesHealthCheckOptions
    {
        private const string DefaultNamespace = "default";
        internal KubernetesHealthCheckOptions(){}
        internal ConcurrentDictionary<KubernetesResource, Func<V1Deployment, bool>> DeploymentRegistrations  { get; private set; }
      
        internal ConcurrentDictionary<KubernetesResource, Func<V1Pod, bool>> PodRegistrations { get; private set; }

        internal ConcurrentDictionary<KubernetesResource, Func<V1Service, bool>> ServiceRegistrations  { get; private set; }
       

        public KubernetesHealthCheckOptions CheckDeployment(string name, Func<V1Deployment, bool> condition,
            string @namespace = DefaultNamespace)
        {
            DeploymentRegistrations.TryAdd(
                KubernetesResource.Create(KubernetesResourceType.Deployment, name: name, @namespace: @namespace)
                , condition);

            return this;
        }

        public KubernetesHealthCheckOptions CheckPod(string name, Func<V1Pod, bool> condition,
            string @namespace = DefaultNamespace)
        {
            PodRegistrations.TryAdd(KubernetesResource.Create(KubernetesResourceType.Pod, name: name, @namespace: @namespace)
                , condition);
            return this;
        }

        public KubernetesHealthCheckOptions CheckService(string name, Func<V1Service, bool> condition,
            string @namespace = DefaultNamespace)
        {
            ServiceRegistrations.TryAdd(KubernetesResource.Create(KubernetesResourceType.Service, name: name, @namespace: @namespace)
                , condition);

            return this;
        }
    }
}