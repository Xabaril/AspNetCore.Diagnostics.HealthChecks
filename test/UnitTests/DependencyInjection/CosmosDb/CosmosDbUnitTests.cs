using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using HealthChecks.CosmosDb;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.CosmosDb
{
    public class cosmosdb_registration_should
    {
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("myconnectionstring");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured_with_database()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("myconnectionstring", "dabasename");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("myconnectionstring", name: "my-cosmosdb-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-cosmosdb-group");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }

        [Fact]
        public void add_azuretable_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable("myconnectionstring", "tableName");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
        [Fact]
        public void add_azuretable_health_check_when_properly_configured_with_database()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable("myconnectionstring", "tableName");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
        [Fact]
        public void add_azuretable_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable("myconnectionstring", "tableName", name: "my-azuretable-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azuretable-group");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
    }
}
