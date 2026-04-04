using System.Net;
using Milvus.Client;

namespace HealthChecks.Milvus.Tests.Functional;

public class milvus_healthcheck_should(MilvusContainerFixture milvusContainerFixture) : IClassFixture<MilvusContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_milvus_is_available_using_client_factory()
    {
        string connectionString = milvusContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddMilvus(
                            clientFactory: sp => new MilvusClient(new Uri(connectionString)), tags: new string[] { "milvus" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("milvus")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_milvus_is_available_using_singleton()
    {
        string connectionString = milvusContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddSingleton(new MilvusClient(new Uri(connectionString)))
                        .AddHealthChecks()
                        .AddMilvus(tags: new string[] { "milvus" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("milvus")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_milvus_is_unavailable()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddMilvus(
                            clientFactory: sp => new MilvusClient("255.255.255.255"), tags: new string[] { "milvus" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("milvus")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
