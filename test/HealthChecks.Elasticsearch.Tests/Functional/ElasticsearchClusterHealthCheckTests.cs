using System.Net;

namespace HealthChecks.Elasticsearch.Tests.Functional
{
    public class elasticsearch_cluster_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_elasticsearch_is_available()
        {
            var connectionString = @"http://localhost:9200";

            var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddElasticsearchCluster(connectionString, tags: new string[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_elasticsearch_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddElasticsearchCluster("nonexistingdomain:9200", tags: new string[] { "elasticsearch" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("elasticsearch")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .ShouldBe(HttpStatusCode.ServiceUnavailable);
        }
    }
}
