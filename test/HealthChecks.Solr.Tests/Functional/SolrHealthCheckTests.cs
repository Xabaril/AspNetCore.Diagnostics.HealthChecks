using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.Solr.Tests.Functional;

public class solr_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_solr_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                   webHostBuilder.Services.AddHealthChecks()
                    .AddSolr("http://localhost:8983/solr", "solrcore", tags: ["solr"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("solr"),
                       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                   });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_solr_ping_is_disabled()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSolr("http://localhost:8893/solr", "solrcoredown", tags: ["solr"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("solr"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_solr_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSolr("http://200.0.0.100:8893", "core", tags: ["solr"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("solr"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }
}
