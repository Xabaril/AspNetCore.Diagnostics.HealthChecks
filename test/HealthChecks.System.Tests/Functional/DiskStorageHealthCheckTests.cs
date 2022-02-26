using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.System.Tests.Functional
{
    [Collection("execution")]
    public class disk_storage_healthcheck_should
    {
        private readonly DriveInfo[] _drives = DriveInfo.GetDrives();

        [Fact]
        public async Task be_healthy_when_disks_have_more_free_space_than_configured()
        {
            var testDrive = _drives.FirstOrDefault(d => d.DriveType == DriveType.Fixed);

            var testDriveActualFreeMegabytes = testDrive.AvailableFreeSpace / 1024 / 1024;
            var targetFreeSpace = testDriveActualFreeMegabytes - 50;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(setup => setup.AddDrive(testDrive.Name, targetFreeSpace), tags: new string[] { "diskstorage" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("diskstorage")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
              .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_unhealthy_when_a_disk_has_less_free_space_than_configured()
        {
            var testDrive = _drives.FirstOrDefault(d => d.DriveType == DriveType.Fixed);

            var testDriveActualFreeMegabytes = testDrive.AvailableFreeSpace / 1024 / 1024;
            var targetFreeSpace = testDriveActualFreeMegabytes + 50;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(setup => setup.AddDrive(testDrive.Name, targetFreeSpace), tags: new string[] { "diskstorage" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("diskstorage")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
              .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_when_a_configured_disk_does_not_exist()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(setup => setup.AddDrive("nonexistingdisk", 104857600), tags: new string[] { "diskstorage" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("diskstorage")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
              .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
