using System.Net;

namespace HealthChecks.SurrealDb.Tests.Functional;

public class surrealdb_healthcheck_should(SurrealDbContainerFixture surrealDbFixture) : IClassFixture<SurrealDbContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_surrealdb_is_available()
    {
        string connectionString = surrealDbFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddSurreal(connectionString);
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddSurreal(tags: ["surrealdb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("surrealdb")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_surrealdb_is_not_available()
    {
        const string connectionString = "Server=http://localhost:1234;Username=root;Password=root";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddSurreal(connectionString);
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddSurreal(tags: ["surrealdb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("surrealdb")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
