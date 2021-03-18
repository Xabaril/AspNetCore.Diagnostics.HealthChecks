using Azure.Messaging.EventHubs;
using FluentAssertions;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_event_hub_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured_using_connectionstring_and_eventhubname()
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
        public void add_health_check_when_properly_configured_using_eventhubconnectionfactory()
        {
            Func<IServiceProvider,EventHubConnection> factory = 
                _ => new EventHubConnection("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=", "hubname");
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(factory);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhub");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }


        [Fact]
        public void add_named_health_check_when_properly_configured_using_connectionstring_and_eventhubname()
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
        public void add_named_health_check_when_properly_configured_using_connectionfactory()
        {
            Func<IServiceProvider, EventHubConnection> factory =
                _ => new EventHubConnection("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=", "hubname");
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(factory, name: "azureeventhubcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhub");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
