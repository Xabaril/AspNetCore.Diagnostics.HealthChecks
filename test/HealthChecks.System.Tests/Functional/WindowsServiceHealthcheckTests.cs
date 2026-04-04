using System.Net;
using System.ServiceProcess;
using HealthChecks.System.Tests.Seedwork;

namespace HealthChecks.System.Tests.Functional;

[Collection("execution")]
[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows only")]
public class windows_service__healthcheck_should
{
    [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
    public async Task be_healthy_when_the_service_is_running()
    {
        var webhostBuilder = WebApplication.CreateBuilder();
                    webhostBuilder.Services.AddHealthChecks()
                        .AddWindowsServiceHealthCheck("Windows Update", s => s.StartType == ServiceStartMode.Manual);
        webhostBuilder.WebHost.UseTestServer();
        await using var app = webhostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health").ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
    public async Task be_unhealthy_when_the_service_does_not_exist()
    {
        var webhostBuilder = WebApplication.CreateBuilder();
                    webhostBuilder.Services.AddHealthChecks()
                        .AddWindowsServiceHealthCheck("someservice", s => s.Status == ServiceControllerStatus.Running);
        webhostBuilder.WebHost.UseTestServer();
        await using var app = webhostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health").ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [SkipOnPlatform(Platform.WINDOWS)]
    public async Task throw_exception_when_registering_it_in_a_no_windows_system()
    {
        var webhostBuilder = WebApplication.CreateBuilder();
                    webhostBuilder.Services.AddHealthChecks()
                        .AddWindowsServiceHealthCheck("dotnet", s => s.Status == ServiceControllerStatus.Running);
        webhostBuilder.WebHost.UseTestServer();
        await using var app = webhostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true
                    });
        await app.StartAsync();

        var exception = Should.Throw<PlatformNotSupportedException>(() =>
        {
        });

        exception.Message.ShouldBe("WindowsServiceHealthCheck can only be registered in Windows Systems");
    }
}
