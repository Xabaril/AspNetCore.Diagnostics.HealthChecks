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
    public class azure_service_bus_topic_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic("cnn", "topicName");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopic");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic("cnn", "topic",
                name: "azuretopiccheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopiccheck");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
        }

        [Fact]
        public void register_succeeded_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var healthCheck = registration.Factory(serviceProvider);
            healthCheck.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
        }

        [Theory]
        [InlineData("", "test", HealthStatus.Degraded)]
        [InlineData("test", "", HealthStatus.Degraded)]
        [InlineData("", "", HealthStatus.Unhealthy)]
        [InlineData("", "", HealthStatus.Healthy)]
        public async Task empty_string_check_returns_failure_status(string connectionString, string topicName, HealthStatus status)
        {
            // Arrange
            var healthCheck = new AzureServiceBusTopicHealthCheck(connectionString, topicName);
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
