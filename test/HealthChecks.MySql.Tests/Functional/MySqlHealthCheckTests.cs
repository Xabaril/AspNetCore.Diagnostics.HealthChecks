using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace HealthChecks.MySql.Tests.Functional
{
    public class mysql_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_when_mysql_server_is_available()
        {
            var connectionString = "server=localhost;port=3306;database=information_schema;uid=root;password=Password12!";

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                   .AddMySql(connectionString, tags: new string[] { "mysql" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("mysql")
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_when_mysql_server_is_unavailable()
        {
            var connectionString = "server=255.255.255.255;port=3306;database=information_schema;uid=root;password=Password12!";

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddMySql(connectionString, tags: new string[] { "mysql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("mysql")
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
