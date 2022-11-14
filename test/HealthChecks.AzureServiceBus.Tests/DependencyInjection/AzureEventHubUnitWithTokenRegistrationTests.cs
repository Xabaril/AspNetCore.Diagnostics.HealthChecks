using Azure.Identity;

namespace HealthChecks.AzureServiceBus.Tests
{
    public class azure_event_hub_registration_with_token_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub("cnn", "eventHubName", new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhub");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
        }

        [Fact]
        public void add_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false, eventHubNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureEventHub(_ =>
                {
                    endpointFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    eventHubNameFactoryCalled = true;
                    return "eventHubName";
                },
                _ =>
                {
                    tokenCredentialFactoryCalled = true;
                    return new AzureCliCredential();
                });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhub");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
            endpointFactoryCalled.ShouldBeTrue();
            eventHubNameFactoryCalled.ShouldBeTrue();
            tokenCredentialFactoryCalled.ShouldBeTrue();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub("cnn", "eventHubName", new AzureCliCredential(),
                name: "azureeventhubcheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhubcheck");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false, eventHubNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureEventHub(_ =>
                    {
                        endpointFactoryCalled = true;
                        return "cnn";
                    },
                    _ =>
                    {
                        eventHubNameFactoryCalled = true;
                        return "eventHubName";
                    },
                    _ =>
                    {
                        tokenCredentialFactoryCalled = true;
                        return new AzureCliCredential();
                    },
                    "azureeventhubcheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureeventhubcheck");
            check.ShouldBeOfType<AzureEventHubHealthCheck>();
            endpointFactoryCalled.ShouldBeTrue();
            eventHubNameFactoryCalled.ShouldBeTrue();
            tokenCredentialFactoryCalled.ShouldBeTrue();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(string.Empty, string.Empty, new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
