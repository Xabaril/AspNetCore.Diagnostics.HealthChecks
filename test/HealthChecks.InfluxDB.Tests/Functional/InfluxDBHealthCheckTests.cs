using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.InfluxDB.Tests.Functional
{
    public class ibmmq_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_influxdb_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user", "password".ToCharArray(), "influxdb", tags: new string[] { "influxdb" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("influxdb")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_influxdb_is_unavailable()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddHealthChecks()
                        .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user_unavailable", "password".ToCharArray(), "influxdb", tags: new string[] { "influxdb" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("influxdb")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

    }
}
