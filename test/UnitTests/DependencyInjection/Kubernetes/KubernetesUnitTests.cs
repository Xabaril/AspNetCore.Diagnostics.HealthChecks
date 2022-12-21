using HealthChecks.Kubernetes;
using k8s;

namespace UnitTests.HealthChecks.DependencyInjection.Kubernetes
{
    public class kubernetes_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddKubernetes(setup =>
                {
                    setup.WithConfiguration(new KubernetesClientConfiguration
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer");
                });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("k8s");
            check.ShouldBeOfType<KubernetesHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddKubernetes(setup =>
                {
                    setup.WithConfiguration(new KubernetesClientConfiguration
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer");
                }, name: "second-k8s-cluster");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("second-k8s-cluster");
            check.ShouldBeOfType<KubernetesHealthCheck>();
        }
    }
}
