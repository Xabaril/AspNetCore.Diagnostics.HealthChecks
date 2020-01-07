using System;
using System.Linq;
using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using HealthChecks.GCP.CloudStorage;
using HealthChecks.Gcp.CloudStorage.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.CloudStorage
{
    public class CloudStorageUnitTest
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {

            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddGcpCloudStorage(setup =>
                {
                    setup.ProjectId = Guid.NewGuid().ToString();
                    setup.Bucket = "mybucket";
                    setup.GoogleCredential = GoogleCredential.FromAccessToken("xxxxxxxxx");
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("gcpcloudstorage");
            check.GetType().Should().Be(typeof(GcpCloudStorageHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();

            services.AddHealthChecks()
                .AddGcpCloudStorage(setup =>
                {
                    setup.ProjectId = Guid.NewGuid().ToString();
                    setup.Bucket = "mybucket";
                    setup.GoogleCredential = GoogleCredential.FromAccessToken("xxxxxxxxx");
                }, name: "my-cloud-storage-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-cloud-storage-group");
            check.GetType().Should().Be(typeof(GcpCloudStorageHealthCheck));
        }
    }
}