using System;
using k8s;

namespace HealthChecks.Kubernetes
{
    public class KubernetesResource
    {
        public KubernetesResourceType ResourceType { get; set; }
        public string Name { get; }
        public string Namespace { get; }

        private KubernetesResource(KubernetesResourceType  type, string name, string @namespace)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            ResourceType = type;
        }

        public static KubernetesResource Create(KubernetesResourceType type, string name, string @namespace) =>
            
            new KubernetesResource(type, name, @namespace);
    }
}