using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_event_hub_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azureeventhub", typeof(AzureEventHubHealthCheck), builder => builder.AddAzureEventHub(
                "Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=",
                "hubName"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("azureeventhubcheck", typeof(AzureEventHubHealthCheck), builder => builder.AddAzureEventHub(
                "Endpoint=sb://dummynamespace.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=",
                "hubName", name: "azureeventhubcheck"));
        }
        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            ShouldThrow<ArgumentNullException>(builder => builder.AddAzureEventHub(string.Empty, string.Empty));
        }
    }
}
