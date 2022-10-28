using k8s.Models;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    public class DeploymentResult
    {
        public V1Deployment Deployment { get; private set; }
        public V1Service Service { get; private set; }
        public V1Secret Secret { get; set; }

        private DeploymentResult(V1Deployment deployment, V1Service service, V1Secret secret)
        {
            Deployment = Guard.ThrowIfNull(deployment);
            Service = Guard.ThrowIfNull(service);
            Secret = Guard.ThrowIfNull(secret);
        }

        public static DeploymentResult Create(V1Deployment deployment, V1Service service, V1Secret secret)
        {
            return new DeploymentResult(deployment, service, secret);
        }
    }
}
