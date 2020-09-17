using FluentAssertions;
using HealthChecks.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using HealthChecks.Neo4j;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Neo4j
{
    public class neo4j_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddNeo4j(_ => new Neo4jOptions());

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("neo4j");
            check.GetType().Should().Be(typeof(Neo4jHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddNeo4j(_ => new Neo4jOptions(), name: "my-neo4j-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-neo4j-group");
            check.GetType().Should().Be(typeof(Neo4jHealthCheck));
        }
    }
}
