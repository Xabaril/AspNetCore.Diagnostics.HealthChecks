using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
using HealthChecks.ArangoDb;
using Xunit;

namespace FunctionalTests.HealthChecks.ArangoDb
{
    [Collection("execution")]
    public class arangodb_healthcheck_should
    {
        [SkipOnAppVeyor]
        public async Task be_healthy_if_arangodb_is_available()
        {
             var webHostBuilder = new WebHostBuilder()
                 .UseStartup<DefaultStartup>()
                 .ConfigureServices(services =>
                 {
                     services.AddHealthChecks()
                      .AddArangoDb(_ => new ArangoDbOptions
                      {
                          HostUri = "http://localhost:8529/",
                          Database = "_system",
                          UserName = "root",
                          Password = "strongArangoDbPassword"
                      }, tags: new string[] { "arangodb" });
                 })
                 .Configure(app =>
                 {
                     app.UseHealthChecks("/health", new HealthCheckOptions
                     {
                         Predicate = r => r.Tags.Contains("arangodb")
                     });
                 });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_multiple_arango_are_available()
        {
             var webHostBuilder = new WebHostBuilder()
                 .UseStartup<DefaultStartup>()
                 .ConfigureServices(services =>
                 {
                     services.AddHealthChecks()
                      .AddArangoDb(_ => new ArangoDbOptions
                      {
                          HostUri = "http://localhost:8529/",
                          Database = "_system",
                          UserName = "root",
                          Password = "strongArangoDbPassword"
                      }, tags: new string[] { "arango" }, name: "1")
                      .AddArangoDb(_ =>new ArangoDbOptions
                      {
                          HostUri = "http://localhost:8529/",
                          Database = "_system",
                          UserName = "root",
                          Password = "strongArangoDbPassword",
                          IsGenerateJwtTokenBasedOnUserNameAndPassword = true
                      }, tags: new string[] { "arango" }, name: "2");
                 })
                 .Configure(app =>
                 {
                     app.UseHealthChecks("/health", new HealthCheckOptions
                     {
                         Predicate = r => r.Tags.Contains("arango")
                     });
                 });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_arango_is_not_available()
        {
             var webHostBuilder = new WebHostBuilder()
                 .UseStartup<DefaultStartup>()
                 .ConfigureServices(services =>
                 {
                     services.AddHealthChecks()
                      .AddArangoDb(_ => new ArangoDbOptions
                      {
                          HostUri = "http://localhost:8529/",
                          Database = "_system",
                          UserName = "root",
                          Password = "invalid password"
                      }, tags: new string[] { "arango" });
                 })
                 .Configure(app =>
                 {
                     app.UseHealthChecks("/health", new HealthCheckOptions
                     {
                         Predicate = r => r.Tags.Contains("arango")
                     });
                 });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
