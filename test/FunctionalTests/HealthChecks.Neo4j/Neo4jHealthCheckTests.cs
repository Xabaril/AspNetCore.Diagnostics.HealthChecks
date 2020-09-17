using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using HealthChecks.Neo4j;
using Xunit;

namespace FunctionalTests.HealthChecks.Neo4j
{
    [Collection("execution")]
    public class neo4j_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public neo4j_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentException(nameof(fixture));
        }

        [Fact]
        public async Task be_healthy_when_neo4j_server_is_available()
        {
            var options = new Neo4jOptions
            {
                Uri = "neo4j://localhost:7687",
                UserName = "neo4j",
                Password = "p@$$wort"
            };

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                   .AddNeo4j(_ => options, tags: new[] { "test-neo4j" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("test-neo4j")
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health").GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_when_neo4j_server_login_password_invalid()
        {
            var options = new Neo4jOptions
            {
                Uri = "neo4j://localhost:7687",
                UserName = "wrong-user",
                Password = "wrong-password"
            };

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddNeo4j(_ => options, tags: new[] { "test-neo4j" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("test-neo4j")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should()
                .Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_when_neo4j_server_is_unavailable()
        {
            var options = new Neo4jOptions
            {
                Uri = "not-neo4j://localhost:7687",
                UserName = "neo4j",
                Password = "p@$$wort"
            };

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddNeo4j(_ => options, tags: new[] { "test-neo4j" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("test-neo4j")
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