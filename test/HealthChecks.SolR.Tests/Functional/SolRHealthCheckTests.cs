using System.Net;
using FluentAssertions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.SolR.Tests.Functional
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
