using System.Net;

namespace HealthChecks.Elasticsearch.Tests.Functional;

public class elasticsearch_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_elasticsearch_is_available()
    {
        // TODO: Port 9200 is Secure BUT will throw CA Certificate error if used with https
        // `javax.net.ssl.SSLHandshakeException: Received fatal alert: unknown_ca`
        // TODO: http://localhost:9200 results in a 503
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = new WebHostBuilder()
        .ConfigureServices(services =>
        {
            services.AddHealthChecks()
             .AddElasticsearch(connectionString, tags: new string[] { "elasticsearch" });
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
                .AddElasticsearch("https://nonexistingdomain:9200", tags: new string[] { "elasticsearch" });
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
