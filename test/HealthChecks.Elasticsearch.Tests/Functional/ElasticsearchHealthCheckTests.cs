using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace HealthChecks.Elasticsearch.Tests.Functional
{
    public class elasticsearch_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_elasticsearch_is_available()
        {
            var connectionString = @"http://localhost:9200";

            var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddElasticsearch(connectionString, tags: new string[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_elasticsearch_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddElasticsearch("nonexistingdomain:9200", tags: new string[] { "elasticsearch" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("elasticsearch")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
