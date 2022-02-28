using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace HealthChecks.ArangoDb.Tests.Functional
{
    public class arangodb_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_arangodb_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
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

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_multiple_arango_are_available()
        {
            var webHostBuilder = new WebHostBuilder()
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
                        .AddArangoDb(_ => new ArangoDbOptions
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

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_arango_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
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

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
