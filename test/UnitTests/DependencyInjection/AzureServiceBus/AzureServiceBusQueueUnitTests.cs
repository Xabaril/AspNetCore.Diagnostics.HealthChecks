using FluentAssertions;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_service_bus_queue_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue("cnn", "queueName");

            var serviceProvider = services.BuildServiceProvider();
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
                .AddAzureServiceBusQueue("cnn", "queueName",
                name: "azureservicebusqueuecheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureservicebusqueuecheck");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
        }

        [Fact]
        public void register_succeeded_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var healthCheck = registration.Factory(serviceProvider);
            healthCheck.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
        }

        [Theory]
        [InlineData("", "test", HealthStatus.Degraded)]
        [InlineData("test", "", HealthStatus.Degraded)]
        [InlineData("", "", HealthStatus.Unhealthy)]
        [InlineData("", "", HealthStatus.Healthy)]
        public async Task empty_string_check_returns_failure_status(string connectionString, string queueName, HealthStatus status)
        {
            // Arrange
            var healthCheck = new AzureServiceBusQueueHealthCheck(connectionString, queueName);
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
