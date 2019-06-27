using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;
using global::Azure.Identity;

namespace HealthChecks.AzureServiceBus.Tests
{
    public class azure_service_bus_queue_registration_with_token_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue("cnn", "queueName", new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));

        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue("cnn", "queueName", new AzureCliCredential(),
                name: "azureservicebusqueuecheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureservicebusqueuecheck");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(string.Empty, string.Empty, new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }

        [Fact]
        public void add_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool connectionStringFactoryCalled = false, eventHubNameFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureEventHub(_ =>
                    {
                        connectionStringFactoryCalled = true;
                        return "Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=";
                    },
                    _ =>
                    {
                        eventHubNameFactoryCalled = true;
                        return "hubName";
                    });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhub");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
            connectionStringFactoryCalled.Should().BeTrue();
            eventHubNameFactoryCalled.Should().BeTrue();
        }
    }
}
