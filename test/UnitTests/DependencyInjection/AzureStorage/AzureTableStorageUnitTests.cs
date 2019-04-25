using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.AzureStorage
{
    public class azuretabletorage_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azuretable", typeof(AzureTableStorageHealthCheck), builder => builder.AddAzureTableStorage(
                "the-connection-string"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-azuretable-group", typeof(AzureTableStorageHealthCheck), builder => builder.AddAzureTableStorage(
                "the-connection-string", name: "my-azuretable-group"));
        }
    }
}
