using FluentAssertions;
using global::Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.AzureServiceBus.Tests
{
    public class azure_service_bus_subscription_registration_with_token_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusSubscription("cnn", "topicName", "subscriptionName", new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuresubscription");
            check.GetType().Should().Be(typeof(AzureServiceBusSubscriptionHealthCheck));
        }

        [Fact]
        public void add_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false,
                topicNameFactoryCalled = false,
                subscriptionNameFactoryCalled = false,
                tokenCredentialsFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusSubscription(_ =>
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
                        subscriptionNameFactoryCalled = true;
                        return "subscriptionName";
                    },
                    _ =>
                    {
                        tokenCredentialsFactoryCalled = true;
                        return new AzureCliCredential();
                    });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuresubscription");
            check.GetType().Should().Be(typeof(AzureServiceBusSubscriptionHealthCheck));
            endpointFactoryCalled.Should().BeTrue();
            topicNameFactoryCalled.Should().BeTrue();
            subscriptionNameFactoryCalled.Should().BeTrue();
            tokenCredentialsFactoryCalled.Should().BeTrue();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusSubscription("cnn", "topic", "subscriptionName", new AzureCliCredential(),
                name: "azuresubscriptioncheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuresubscriptioncheck");
            check.GetType().Should().Be(typeof(AzureServiceBusSubscriptionHealthCheck));
        }

        [Fact]
        public void add_named_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false,
                topicNameFactoryCalled = false,
                subscriptionNameFactoryCalled = false,
                tokenCredentialsFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusSubscription(_ =>
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
                        subscriptionNameFactoryCalled = true;
                        return "subscriptionName";
                    },
                    _ =>
                    {
                        tokenCredentialsFactoryCalled = true;
                        return new AzureCliCredential();
                    },
                    "azuresubscriptioncheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuresubscriptioncheck");
            check.GetType().Should().Be(typeof(AzureServiceBusSubscriptionHealthCheck));
            endpointFactoryCalled.Should().BeTrue();
            topicNameFactoryCalled.Should().BeTrue();
            subscriptionNameFactoryCalled.Should().BeTrue();
            tokenCredentialsFactoryCalled.Should().BeTrue();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusSubscription(string.Empty, string.Empty, string.Empty, new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
