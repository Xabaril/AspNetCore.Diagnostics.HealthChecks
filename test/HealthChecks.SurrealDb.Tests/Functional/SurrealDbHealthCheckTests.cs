using System.Net;

namespace HealthChecks.SurrealDb.Tests.Functional;

public class surrealdb_healthcheck_should(SurrealDbContainerFixture surrealDbFixture) : IClassFixture<SurrealDbContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_surrealdb_is_available()
    {
        string connectionString = surrealDbFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSurreal(connectionString);
                services
                    .AddHealthChecks()
                    .AddSurreal(tags: ["surrealdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("surrealdb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_surrealdb_is_not_available()
    {
        const string connectionString = "Server=http://localhost:1234;Username=root;Password=root";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSurreal(connectionString);
                services
                    .AddHealthChecks()
                    .AddSurreal(tags: ["surrealdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("surrealdb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
