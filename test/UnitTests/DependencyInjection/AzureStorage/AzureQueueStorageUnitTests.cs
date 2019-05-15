using FluentAssertions;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace UnitTests.DependencyInjection.AzureStorage
{
    public class azurequeuestorage_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azurequeue", typeof(AzureQueueStorageHealthCheck), builder => builder.AddAzureQueueStorage(
                "the-connection-string"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-azurequeue-group", typeof(AzureQueueStorageHealthCheck), builder => builder.AddAzureQueueStorage(
                "the-connection-string", name: "my-azurequeue-group"));
        }
        [Fact] 
        public void add_custom_tagged_health_check_when_properly_configured() 
        {
            ShouldPass(builder => builder.AddAzureQueueStorage("the-connection-string", name: "my-azurequeue-group", tags: new[] { "custom-tag" }),
                (registration, check) =>
                {
                    registration.Name.Should().Be("my-azurequeue-group");
                    registration.Tags.Should().Contain("custom-tag");
                    check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
                });
        }
        [Fact] 
        public void add_health_check_with_custom_failure_status_when_properly_configured() 
        {
            ShouldPass(builder => builder.AddAzureQueueStorage("the-connection-string", name: "my-azurequeue-group", failureStatus: HealthStatus.Degraded),
               (registration, check) =>
               {
                   registration.Name.Should().Be("my-azurequeue-group");
                   registration.FailureStatus.Should().Be(HealthStatus.Degraded);
                   check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
               });
        }
    }
}
