using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace HealthChecks.IdSvr.Tests.Functional
{
    public class idsvr_healthcheck_should
    {
        [Fact]
        public async Task be_unhealthy_if_idsvr_is_unavailable()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddIdentityServer(new Uri("http://localhost:7777"), tags: new string[] { "idsvr" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("idsvr")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_idsvr_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddIdentityServer(new Uri("http://localhost:8888"), tags: new string[] { "idsvr" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("idsvr")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }
    }
}
