using System.Net;

namespace HealthChecks.System.Tests.Functional;

public class file_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_file_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                   webHostBuilder.Services.AddHealthChecks()
                       .AddFile(setup => setup.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "HealthChecks.System.Tests.dll")), tags: ["file"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("file")
                   });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_no_file_provided()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                   webHostBuilder.Services.AddHealthChecks()
                       .AddFile(setup =>
                       {
                       }, tags: ["file"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("file")
                   });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_file_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddFile(setup => setup.AddFile($"{Directory.GetCurrentDirectory()}/non-existing-file"), tags: ["file"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("file")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
