using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.Solr.Tests.Functional
{
    public class solr_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_solr_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddSolr("http://localhost:8983/solr", "solrcore", tags: new string[] { "solr" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("solr"),
                       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                   });
               });

            using var server = new TestServer(webHostBuilder);
            using var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task be_unhealthy_if_solr_ping_is_disabled()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSolr("http://localhost:8893/solr", "solrcoredown", tags: new string[] { "solr" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("solr"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                });

            using var server = new TestServer(webHostBuilder);
            using var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task be_unhealthy_if_solr_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSolr("http://200.0.0.100:8893", "core", tags: new string[] { "solr" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("solr"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                });

            using var server = new TestServer(webHostBuilder);
            using var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}
