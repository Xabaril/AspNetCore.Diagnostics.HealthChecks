using FluentAssertions;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTests.DependencyInjection.AzureServiceBus;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_service_bus_topic_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string topicName = "topicName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(connectionString, topicName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopic");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
            var serviceBusTopicHealthCheck = check as AzureServiceBusTopicHealthCheck;
            serviceBusTopicHealthCheck.Should().NotBeNull();
            serviceBusTopicHealthCheck.Endpoint.Contains(namespaceName);
            serviceBusTopicHealthCheck.EntityPath.Should().Be(topicName);
        }

        [Fact]
        public void add_health_check_without_topic_name_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string topicName = "topicName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName, topicName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(connectionString);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopic");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
            var serviceBusTopicHealthCheck = check as AzureServiceBusTopicHealthCheck;
            serviceBusTopicHealthCheck.Should().NotBeNull();
            serviceBusTopicHealthCheck.Endpoint.Contains(namespaceName);
            serviceBusTopicHealthCheck.EntityPath.Should().Be(topicName);
        }
        [Fact]
        public void add_health_check_with_sessions_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string topicName = "topicName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(connectionString, topicName, requiresSession: true);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopic");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
            var serviceBusTopicHealthCheck = check as AzureServiceBusTopicHealthCheck;
            serviceBusTopicHealthCheck.Should().NotBeNull();
            serviceBusTopicHealthCheck.RequiresSession.Should().BeTrue();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string topicName = "topicName";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(connectionString, topicName,
                    name: "azuretopiccheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretopiccheck");
            check.GetType().Should().Be(typeof(AzureServiceBusTopicHealthCheck));
            var serviceBusTopicHealthCheck = check as AzureServiceBusTopicHealthCheck;
            serviceBusTopicHealthCheck.Should().NotBeNull();
            serviceBusTopicHealthCheck.EntityPath.Should().Be(topicName);
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusTopic(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
