using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_service_bus_topic_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azuretopic", typeof(AzureServiceBusTopicHealthCheck), builder => builder.AddAzureServiceBusTopic(
                "cnn", "topicName"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("azuretopiccheck", typeof(AzureServiceBusTopicHealthCheck), builder => builder.AddAzureServiceBusTopic(
                "cnn", "topic",
                name: "azuretopiccheck"));
        }
        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            ShouldThrow<ArgumentNullException>(builder => builder.AddAzureServiceBusTopic(string.Empty, string.Empty));
        }
    }
}
