using FluentAssertions;
using HealthChecks.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using HealthChecks.ArangoDb;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.ArangoDb
{
    public class arangodb_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddArangoDb(_ => new ArangoDbOptions());

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("arangodb");
            check.GetType().Should().Be(typeof(ArangoDbHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddArangoDb(_ => new ArangoDbOptions(), name: "my-arango");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-arango");
            check.GetType().Should().Be(typeof(ArangoDbHealthCheck));
        }
    }
}
