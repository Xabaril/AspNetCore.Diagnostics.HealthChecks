using System.Net;

namespace HealthChecks.System.Tests.Functional;

[Collection("execution")]
public class disk_storage_healthcheck_should
{
    private readonly DriveInfo[] _drives = DriveInfo.GetDrives();

    [Fact]
    public async Task be_healthy_when_disks_have_more_free_space_than_configured()
    {
        var testDrive = _drives.First(d => d.DriveType == DriveType.Fixed);

        var testDriveActualFreeMegabytes = testDrive.AvailableFreeSpace / 1024 / 1024;
        var targetFreeSpace = testDriveActualFreeMegabytes - 50;

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(setup => setup.AddDrive(testDrive.Name, targetFreeSpace), tags: ["diskstorage"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("diskstorage")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_a_disk_has_less_free_space_than_configured()
    {
        var testDrive = _drives.First(d => d.DriveType == DriveType.Fixed);

        var testDriveActualFreeMegabytes = testDrive.AvailableFreeSpace / 1024 / 1024;
        var targetFreeSpace = testDriveActualFreeMegabytes + 50;

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(setup => setup.AddDrive(testDrive.Name, targetFreeSpace), tags: ["diskstorage"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("diskstorage")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_a_configured_disk_does_not_exist()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(setup => setup.AddDrive("nonexistingdisk", 104857600), tags: ["diskstorage"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("diskstorage")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
