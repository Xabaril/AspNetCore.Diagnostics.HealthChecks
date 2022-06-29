using global::Azure.Identity;

namespace HealthChecks.AzureServiceBus.Tests
{
    public class azure_service_bus_topic_registration_with_token_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic("cnn", "topicName", new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopic");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
        }

        [Fact]
        public void add_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false, topicNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(_ =>
                    {
                        endpointFactoryCalled = true;
                        return "cnn";
                    },
                    _ =>
                    {
                        topicNameFactoryCalled = true;
                        return "topicName";
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

            registration.Name.Should().Be("azuretopic");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
            endpointFactoryCalled.Should().BeTrue();
            topicNameFactoryCalled.Should().BeTrue();
            tokenCredentialFactoryCalled.Should().BeTrue();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic("cnn", "topic", new AzureCliCredential(),
                    "azuretopiccheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopiccheck");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
        }

        [Fact]
        public void add_named_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false, topicNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(_ =>
                    {
                        endpointFactoryCalled = true;
                        return "cnn";
                    },
                    _ =>
                    {
                        topicNameFactoryCalled = true;
                        return "topicName";
                    },
                    _ =>
                    {
                        tokenCredentialFactoryCalled = true;
                        return new AzureCliCredential();
                    },
                    "azuretopiccheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopiccheck");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
            endpointFactoryCalled.Should().BeTrue();
            topicNameFactoryCalled.Should().BeTrue();
            tokenCredentialFactoryCalled.Should().BeTrue();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(string.Empty, string.Empty, new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
