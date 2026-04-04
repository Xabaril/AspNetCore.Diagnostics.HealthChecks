using System.Net;
using Qdrant.Client;

namespace HealthChecks.Qdrant.Tests.Functional;

public class qdrant_healthcheck_should(QdrantContainerFixture qdrantContainerFixture) : IClassFixture<QdrantContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_qdrant_is_available_using_client_factory()
    {
        string connectionString = qdrantContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddQdrant(
                            clientFactory: sp => new QdrantClient(new Uri(connectionString)), tags: new string[] { "qdrant" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("qdrant")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_qdrant_is_available_using_singleton()
    {
        string connectionString = qdrantContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddSingleton(new QdrantClient(new Uri(connectionString)))
                        .AddHealthChecks()
                        .AddQdrant(tags: new string[] { "qdrant" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("qdrant")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_qdrant_is_unavailable()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddQdrant(
                            clientFactory: sp => new QdrantClient("255.255.255.255"), tags: new string[] { "qdrant" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("qdrant")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
