using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.AzureStorage
{
    public class azureblobstorage_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azureblob", typeof(AzureBlobStorageHealthCheck), builder => builder.AddAzureBlobStorage(
                "the-connection-string"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-azureblob-group", typeof(AzureBlobStorageHealthCheck), builder => builder.AddAzureBlobStorage(
                "the-connection-string", name: "my-azureblob-group"));
        }
    }
}
