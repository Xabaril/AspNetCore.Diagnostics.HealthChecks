using System.Net;

namespace HealthChecks.Elasticsearch.Tests.Functional;

public class elasticsearch_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_elasticsearch_is_available()
    {
        var connectionString = @"http://localhost:9201";

        var webHostBuilder = new WebHostBuilder()
        .ConfigureServices(services =>
        {
            services.AddHealthChecks()
             .AddElasticsearch(connectionString, tags: ["elasticsearch"]);
        })
        .Configure(app =>
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("elasticsearch")
            });
        });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_elasticsearch_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddElasticsearch("nonexistingdomain:9200", tags: ["elasticsearch"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
