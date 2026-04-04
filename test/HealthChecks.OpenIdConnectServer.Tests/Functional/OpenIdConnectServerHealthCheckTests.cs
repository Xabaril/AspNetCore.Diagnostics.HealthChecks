using System.Net;

namespace HealthChecks.OpenIdConnectServer.Tests.Functional;

public class oidc_server_healthcheck_should
{
    [Fact]
    public async Task be_unhealthy_if_oidc_server_is_unavailable()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddOpenIdConnectServer(new Uri("http://localhost:7777"), tags: ["oidcserver"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oidcserver")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_oidc_server_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddOpenIdConnectServer(new Uri("http://localhost:8888"), tags: ["oidcserver"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oidcserver")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
