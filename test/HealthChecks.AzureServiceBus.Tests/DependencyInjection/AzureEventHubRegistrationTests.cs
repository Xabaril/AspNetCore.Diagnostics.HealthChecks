using Azure.Messaging.EventHubs;

namespace HealthChecks.AzureServiceBus.Tests
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

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhub");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
        }

        [Fact]
        public void add_health_check_when_properly_configured_using_eventhubconnectionfactory()
        {
            Func<IServiceProvider, EventHubConnection> factory =
                _ => new EventHubConnection("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=", "hubnameconnection");
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(factory);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhub");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured_using_connectionstring_and_eventhubname()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=",
                    "hubName", name: "azureeventhubcheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhubcheck");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured_using_connectionfactory()
        {
            Func<IServiceProvider, EventHubConnection> factory =
                _ => new EventHubConnection("Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=", "hubname");
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(factory, name: "azureeventhubcheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhubcheck");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(string.Empty, string.Empty);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }

        [Fact]
        public void add_health_check_using_connection_string_factory_and_event_hub_name_factory_when_properly_configured()
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

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhub");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
            connectionStringFactoryCalled.ShouldBeTrue();
            eventHubNameFactoryCalled.ShouldBeTrue();
        }
    }
}
