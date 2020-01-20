using System.Linq;
using FluentAssertions;
using HealthChecks.Kubernetes;
using HealthChecks.MongoDb;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

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
                    setup.WithConfiguration(new KubernetesClientConfiguration()
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer");
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("k8s");
            check.GetType().Should().Be(typeof(KubernetesHealthCheck));
        }
        
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddKubernetes(setup =>
                {
                    setup.WithConfiguration(new KubernetesClientConfiguration()
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer");
                }, name: "second-k8s-cluster");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("second-k8s-cluster");
            check.GetType().Should().Be(typeof(KubernetesHealthCheck));
        }
    }
}