using FluentAssertions;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using UnitTests.DependencyInjection.AzureServiceBus;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_service_bus_queue_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string queueName = "queueName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName, queueName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(connectionString, queueName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
            var azureServiceBusQueueHealthCheck = check as AzureServiceBusQueueHealthCheck;
            azureServiceBusQueueHealthCheck.Should().NotBeNull();
            azureServiceBusQueueHealthCheck.Endpoint.Contains(namespaceName);
            azureServiceBusQueueHealthCheck.EntityPath.Should().Be(queueName);
        }

        [Fact]
        public void add_health_check_without_queue_name_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string queueName = "queueName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName, queueName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(connectionString);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
            var azureServiceBusQueueHealthCheck = check as AzureServiceBusQueueHealthCheck;
            azureServiceBusQueueHealthCheck.Should().NotBeNull();
            azureServiceBusQueueHealthCheck.Endpoint.Contains(namespaceName);
            azureServiceBusQueueHealthCheck.EntityPath.Should().Be(queueName);
        }
        [Fact]
        public void add_health_check_with_sessions_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string queueName = "queueName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(connectionString, queueName, requiresSession: true);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
            var azureServiceBusQueueHealthCheck = check as AzureServiceBusQueueHealthCheck;
            azureServiceBusQueueHealthCheck.Should().NotBeNull();
            azureServiceBusQueueHealthCheck.RequiresSession.Should().BeTrue();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string queueName = "queueName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName, queueName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(connectionString, queueName,
                name: "azureservicebusqueuecheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureservicebusqueuecheck");
            check.GetType().Should().Be(typeof(AzureServiceBusQueueHealthCheck));
            var azureServiceBusQueueHealthCheck = check as AzureServiceBusQueueHealthCheck;
            azureServiceBusQueueHealthCheck.Should().NotBeNull();
            azureServiceBusQueueHealthCheck.EntityPath.Should().Be(queueName);
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
