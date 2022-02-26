using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace HealthChecks.Sqlite.Tests.Functional
{
    public class sqlite_healthcheck_should
    {
        [Fact]
        public async void be_healthy_when_sqlite_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSqlite($"Data Source=sqlite.db", healthQuery: "select name from sqlite_master where type='table'", tags: new string[] { "sqlite" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("sqlite")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_when_sqlite_is_unavailable()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSqlite($"Data Source=fake.db", healthQuery: "select * from Users", tags: new string[] { "sqlite" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("sqlite")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async void be_unhealthy_when_sqlquery_is_not_valid()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSqlite($"Data Source=sqlite.db", healthQuery: "select name from invaliddb", tags: new string[] { "sqlite" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("sqlite")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
