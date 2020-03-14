using FluentAssertions;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureStorage
{
    public class azuretabletorage_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTableStorage("the-connection-string");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(AzureTableStorageHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTableStorage("the-connection-string", name: "my-azuretable-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azuretable-group");
            check.GetType().Should().Be(typeof(AzureTableStorageHealthCheck));
        }
        [Fact]
        public void add_named_table_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTableStorage("the-connection-string", tableName: "table");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(AzureTableStorageHealthCheck));
        }
    }
}
