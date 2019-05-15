using HealthChecks.Kubernetes;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.Kubernetes
{
    public class kubernetes_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("k8s", typeof(KubernetesHealthCheck), builder => builder.AddKubernetes(setup =>
            {
                setup.WithConfiguration(new KubernetesClientConfiguration()
                {
                    Host = "https://localhost:443",
                    SkipTlsVerify = true
                }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer");
            }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("second-k8s-cluster", typeof(KubernetesHealthCheck), builder => builder.AddKubernetes(setup =>
            {
                setup.WithConfiguration(new KubernetesClientConfiguration
                {
                    Host = "https://localhost:443",
                    SkipTlsVerify = true
                }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer");
            }, name: "second-k8s-cluster"));
        }
    }
}