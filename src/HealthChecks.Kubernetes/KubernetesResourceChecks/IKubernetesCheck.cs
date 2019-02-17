using System;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kubernetes.KubernetesResourceChecks
{
    internal interface IKubernetesCheck<T> where T: IKubernetesObject
    {
        Task<bool> CheckAsync(KubernetesResource resource, Func<T, bool> condition);
    }
}