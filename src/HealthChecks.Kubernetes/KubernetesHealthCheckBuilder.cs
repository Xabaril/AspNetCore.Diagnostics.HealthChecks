using k8s;

namespace HealthChecks.Kubernetes;

public class KubernetesHealthCheckBuilder
{
    internal KubernetesClientConfiguration Configuration { get; private set; } = null!;

    internal KubernetesHealthCheckOptions Options { get; private set; } = null!;

    public KubernetesHealthCheckOptions WithConfiguration(KubernetesClientConfiguration configuration)
    {
        Configuration = configuration;
        Options = new KubernetesHealthCheckOptions();
        return Options;
    }
}
