using FluentAssertions;
using HealthChecks.RavenDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;
#pragma warning disable 618

namespace UnitTests.HealthChecks.DependencyInjection.RavenDB
{
    public class ravendb_with_single_conection_string_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRavenDB(setup => setup.Urls =  new[] { "http://localhost:8080" });

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
                .AddRavenDB(setup => setup.Urls = new[] { "http://localhost:8080" }, name: "my-ravendb");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-ravendb");
            check.GetType().Should().Be(typeof(RavenDBHealthCheck));
        }
    }
}
