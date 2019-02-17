using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using k8s;
using k8s.Models;

namespace HealthChecks.Kubernetes
{
    
    public class KubernetesHealthCheckOptions
    {
        private const string DefaultNamespace = "default";
        internal KubernetesHealthCheckOptions(){}
        internal List<KubernetesResourceCheck> Registrations  { get; private set; }

        public KubernetesHealthCheckOptions CheckDeployment(string name, Func<IKubernetesObject, bool> condition,
            string @namespace = DefaultNamespace)
        {

            var deploymentCheck = KubernetesResourceCheck.Create<V1Deployment>(name, @namespace, condition);
            Registrations.Add(deploymentCheck);
            
            return this;
        }

        public KubernetesHealthCheckOptions CheckPod(string name, Func<IKubernetesObject, bool> condition,
            string @namespace = DefaultNamespace)
        {
            var podCheck = KubernetesResourceCheck.Create<V1Pod>(name, @namespace, condition);
            Registrations.Add(podCheck);

            return this;
        }

        public KubernetesHealthCheckOptions CheckService(string name, Func<IKubernetesObject, bool> condition,
            string @namespace = DefaultNamespace)
        {
            var serviceCheck = KubernetesResourceCheck.Create<V1Service>(name, @namespace, condition);
            Registrations.Add(serviceCheck);

            return this;
        }
    }
}