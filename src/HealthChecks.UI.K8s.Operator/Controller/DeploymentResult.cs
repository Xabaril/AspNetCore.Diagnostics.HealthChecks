using System;
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
            Deployment = deployment ?? throw new ArgumentNullException(nameof(deployment));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Secret = secret ?? throw new ArgumentNullException(nameof(secret));
        }
        public static DeploymentResult Create(V1Deployment deployment, V1Service service, V1Secret secret)
        {
            return new DeploymentResult(deployment, service, secret);
        }
    }
}