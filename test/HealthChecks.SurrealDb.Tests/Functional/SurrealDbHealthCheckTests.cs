using System.Net;

namespace HealthChecks.SurrealDb.Tests.Functional;

public class surrealdb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_surrealdb_is_available()
    {
        var connectionString = "Server=http://localhost:8000;Namespace=test;Database=test;Username=root;Password=root";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSurreal(connectionString, tags: new string[] { "surrealdb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("surrealdb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_surrealdb_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSurreal("Server=http://200.0.0.100:1234;Namespace=test;Database=test;Username=root;Password=root", tags: new string[] { "surrealdb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("surrealdb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
