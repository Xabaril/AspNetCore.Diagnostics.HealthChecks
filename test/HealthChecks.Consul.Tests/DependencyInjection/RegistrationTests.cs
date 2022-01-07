using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace HealthChecks.Consul.Tests.DependencyInjection
{
    public class consul_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddConsul(setup =>
                {
                    setup.HostName = "hostname";
                    setup.Port = 8500;
                    setup.RequireHttps = false;
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("consul");
            check.GetType().Should().Be(typeof(ConsulHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddConsul(setup =>
                {
                    setup.HostName = "hostname";
                    setup.Port = 8500;
                    setup.RequireHttps = false;
                }, name: "my-consul-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-consul-group");
            check.GetType().Should().Be(typeof(ConsulHealthCheck));
        }
    }
}
