using k8s;

namespace HealthChecks.Kubernetes
{
    public class KubernetesHealthCheckBuilder
    {
        internal KubernetesClientConfiguration Configuration { get; private set; }
        internal KubernetesHealthCheckOptions Options { get; private set; }
        public KubernetesHealthCheckOptions WithConfiguration(KubernetesClientConfiguration configuration)
        {
            Configuration = configuration;
            Options = new KubernetesHealthCheckOptions();
            return Options;
        }
    }
}