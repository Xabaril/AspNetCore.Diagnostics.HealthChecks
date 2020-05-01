using FluentAssertions;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_event_hub_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=", 
                    "hubName");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhub");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=", 
                    "hubName", name: "azureeventhubcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhubcheck");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Fact]
        public void register_succeeded_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var healthCheck = registration.Factory(serviceProvider);
            healthCheck.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Theory]
        [InlineData("", "test", HealthStatus.Degraded)]
        [InlineData("test", "", HealthStatus.Degraded)]
        [InlineData("", "", HealthStatus.Unhealthy)]
        [InlineData("", "", HealthStatus.Healthy)]
        public async Task empty_string_check_returns_failure_status(string connectionString, string eventHubName, HealthStatus status)
        {
            // Arrange
            var healthCheck = new AzureEventHubHealthCheck(connectionString, eventHubName);
            var fixture = new Fixture();
            fixture.Register<IHealthCheck>(() => healthCheck);
            var context = fixture.Create<HealthCheckContext>();
            context.Registration.FailureStatus = status;

            // Act
            var result = await healthCheck.CheckHealthAsync(context);

            // Assert
            result.Status.Should().Be(status);
        }
    }
}
