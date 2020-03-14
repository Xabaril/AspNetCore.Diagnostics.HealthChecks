using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace HealthChecks.UI.K8s.Operator.Crd
{
    public abstract class CustomResourceList<T> : KubernetesObject where T : CustomResource
    {
        public V1ListMeta Metadata { get; set; }
        public List<CustomResource> Items { get; set; }
    }    
}
