using FluentAssertions;
using global::Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

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
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
        }

        [Fact]
        public void add_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false, queueNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(_ =>
                {
                    endpointFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    queueNameFactoryCalled = true;
                    return "queueName";
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

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
            endpointFactoryCalled.Should().BeTrue();
            queueNameFactoryCalled.Should().BeTrue();
            tokenCredentialFactoryCalled.Should().BeTrue();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue("cnn", "queueName", new AzureCliCredential(),
                name: "azureservicebusqueuecheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureservicebusqueuecheck");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
        }

        [Fact]
        public void add_named_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool endpointFactoryCalled = false, queueNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(_ =>
                    {
                        endpointFactoryCalled = true;
                        return "cnn";
                    },
                    _ =>
                    {
                        queueNameFactoryCalled = true;
                        return "queueName";
                    },
                    _ =>
                    {
                        tokenCredentialFactoryCalled = true;
                        return new AzureCliCredential();
                    },
                    "azureservicebusqueuecheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureservicebusqueuecheck");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
            endpointFactoryCalled.Should().BeTrue();
            queueNameFactoryCalled.Should().BeTrue();
            tokenCredentialFactoryCalled.Should().BeTrue();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(string.Empty, string.Empty, new AzureCliCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
