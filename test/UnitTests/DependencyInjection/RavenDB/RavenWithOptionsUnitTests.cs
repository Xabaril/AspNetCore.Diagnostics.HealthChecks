using FluentAssertions;
using HealthChecks.RavenDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.RavenDB
{
    public class ravendb_with_options_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRavenDB(_ => { _.Urls = new[] {"http://localhost:8080", "http://localhost:8081"}; });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("ravendb");
            check.GetType().Should().Be(typeof(RavenDBHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRavenDB(_ => { _.Urls = new[] {"http://localhost:8080", "http://localhost:8081"}; },
                    name: "my-ravendb");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-ravendb");
            check.GetType().Should().Be(typeof(RavenDBHealthCheck));
        }
    }
}
