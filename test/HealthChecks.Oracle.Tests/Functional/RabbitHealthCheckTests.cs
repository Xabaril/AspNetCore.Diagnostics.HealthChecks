using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace HealthChecks.Sqlite.Tests.Functional
{
    public class oracle_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_when_oracle_is_available()
        {
            var connectionString = "Data Source=localhost:1521/xe;User Id=system;Password=oracle";

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddOracle(connectionString, tags: new string[] { "oracle" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_unhealthy_when_oracle_is_not_available()
        {
            var connectionString = "Data Source=255.255.255.255:1521/xe;User Id=system;Password=oracle";

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddOracle(connectionString, tags: new string[] { "oracle" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_when_sql_query_is_not_valid()
        {
            var connectionString = "Data Source=localhost:1521/xe;User Id=system;Password=oracle";
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddOracle(connectionString, "SELECT 1 FROM InvalidDb", tags: new string[] { "oracle" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
