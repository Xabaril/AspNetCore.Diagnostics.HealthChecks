using k8s;
using k8s.Models;

namespace HealthChecks.Kubernetes;

public class KubernetesHealthCheckOptions
{
    private const string DEFAULT_NAMESPACE = "default";

    internal KubernetesHealthCheckOptions()
    {
    }

    internal List<KubernetesResourceCheck> Registrations { get; } = new();

    public KubernetesHealthCheckOptions CheckDeployment(string name, Func<V1Deployment, bool> condition,
        string @namespace = DEFAULT_NAMESPACE)
    {
        Func<IKubernetesObject, bool> delegateCheck = o => condition((V1Deployment)o);

        var deploymentCheck = KubernetesResourceCheck.Create<V1Deployment>(name, @namespace, delegateCheck);

        Registrations.Add(deploymentCheck);

        return this;
    }

    public KubernetesHealthCheckOptions CheckPod(string name, Func<V1Pod, bool> condition,
        string @namespace = DEFAULT_NAMESPACE)
    {
        Func<IKubernetesObject, bool> delegateCheck = o => condition((V1Pod)o);

        var podCheck = KubernetesResourceCheck.Create<V1Pod>(name, @namespace, delegateCheck);

        Registrations.Add(podCheck);

        return this;
    }

    public KubernetesHealthCheckOptions CheckService(string name, Func<V1Service, bool> condition,
        string @namespace = DEFAULT_NAMESPACE)
    {
        Func<IKubernetesObject, bool> delegateCheck = o => condition((V1Service)o);

        var serviceCheck = KubernetesResourceCheck.Create<V1Service>(name, @namespace, delegateCheck);

        Registrations.Add(serviceCheck);

        return this;
    }
}
