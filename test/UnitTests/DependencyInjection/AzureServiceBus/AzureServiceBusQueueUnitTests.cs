using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_service_bus_queue_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azurequeue", typeof(AzureServiceBusQueueHealthCheck), builder => builder.AddAzureServiceBusQueue(
                "cnn", "queueName"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("azureservicebusqueuecheck", typeof(AzureServiceBusQueueHealthCheck), builder => builder.AddAzureServiceBusQueue(
                "cnn", "queueName",
                name: "azureservicebusqueuecheck"));
        }
        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            ShouldThrow<ArgumentNullException>(builder => builder.AddAzureServiceBusQueue(string.Empty, string.Empty));
        }
    }
}
