using System.Net;

namespace HealthChecks.Elasticsearch.Tests.Functional;

public class elasticsearch_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_elasticsearch_is_available()
    {
        var connectionString = @"http://localhost:9201";

        var webHostBuilder = WebApplication.CreateBuilder();
                webHostBuilder.Services.AddHealthChecks()
                 .AddElasticsearch(connectionString, tags: ["elasticsearch"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_elasticsearch_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddElasticsearch("nonexistingdomain:9200", tags: ["elasticsearch"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("elasticsearch")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
