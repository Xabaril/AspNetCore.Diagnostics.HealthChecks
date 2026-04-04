using System.Net;

namespace HealthChecks.Gremlin.Tests.Functional;

public class gremlin_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_gremlin_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "localhost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: ["gremlin"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("gremlin")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_gremlin_are_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "localhost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: ["gremlin"], name: "1")
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "localhost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: ["gremlin"], name: "2");
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("gremlin")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_gremlin_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "wronghost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: ["gremlin"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("gremlin")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
