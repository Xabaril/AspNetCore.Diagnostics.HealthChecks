using Amazon;
using FluentAssertions;
using HealthChecks.Gremlin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Gremlin
{
    public class gremlin_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddGremlin(_ => new GremlinOptions
                {
                    Hostname = "localhost",
                    Port = 8182,
                    EnableSsl = false
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("gremlin");
            check.GetType().Should().Be(typeof(GremlinHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddGremlin(_ => new GremlinOptions
                {
                    Hostname = "localhost",
                    Port = 8182,
                    EnableSsl = false
                },
                name: "my-gremlin");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-gremlin");
            check.GetType().Should().Be(typeof(GremlinHealthCheck));
        }
    }
}
