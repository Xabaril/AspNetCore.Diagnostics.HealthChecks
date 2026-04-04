using System.Net;

namespace HealthChecks.System.Tests.Functional;

public class folder_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_folder_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                   webHostBuilder.Services.AddHealthChecks()
                       .AddFolder(setup => setup.AddFolder(Directory.GetCurrentDirectory()), tags: ["folder"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("folder")
                   });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_no_folder_provided()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                   webHostBuilder.Services.AddHealthChecks()
                       .AddFolder(setup =>
                       {
                       }, tags: ["folder"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("folder")
                   });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_folder_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddFolder(setup => setup.AddFolder($"{Directory.GetCurrentDirectory()}/non-existing-folder"), tags: ["folder"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("folder")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
