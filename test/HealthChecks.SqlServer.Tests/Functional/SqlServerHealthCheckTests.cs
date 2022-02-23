using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace HealthChecks.SqlServer.Tests.Functional
{
    public class sqlserver_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_sqlServer_is_available()
        {
            //read appveyor services default values on
            //https://www.appveyor.com/docs/services-databases/#sql-server-2017 

            var connectionString = "Server=tcp:localhost,5433;Initial Catalog=master;User Id=sa;Password=Password12!";

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddSqlServer(connectionString, tags: new string[] { "sqlserver" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("sqlserver")
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_sqlServer_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddSqlServer("Server=tcp:200.0.0.100,1833;Initial Catalog=master;User Id=sa;Password=Password12!", tags: new string[] { "sqlserver" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("sqlserver")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_sqlquery_spec_is_not_valid()
        {
            //read appveyor services default values on
            //https://www.appveyor.com/docs/services-databases/#sql-server-2017 

            var connectionString = "Server=tcp:localhost,5433;Initial Catalog=master;User Id=sa;Password=Password12!";

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddSqlServer(connectionString, healthQuery: "SELECT 1 FROM [NOT_VALID_DB]", tags: new string[] { "sqlserver" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("sqlserver")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
