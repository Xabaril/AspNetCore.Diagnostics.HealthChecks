using System.Net;
using HealthChecks.Neo4j.DependencyInjection;
using Neo4j.Driver;

namespace HealthChecks.Neo4j.Tests.Functional;

public class neo4j_healthcheck_should(Neo4jContainerFixture neo4jContainerFixture) : IClassFixture<Neo4jContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_neo4j_is_available_using_singleton()
    {
        var connectionString = neo4jContainerFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IDriver>(GraphDatabase.Driver(
                    connectionString,
                    AuthTokens.Basic("neo4j", "password")))
                .AddHealthChecks()
                .AddNeo4j();
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => true
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_neo4j_is_not_available()
    {
        var connectionString = "bolt://localhost:9999";

        var webHostBuilder = new WebHostBuilder()
             .ConfigureServices(services =>
             {
                 services
                    .AddSingleton<IDriver>(GraphDatabase.Driver(connectionString))
                    .AddHealthChecks()
                    .AddNeo4j();
             })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => true
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
