using System.Net;
using Aspire.Milvus.Client.Tests;
using Milvus.Client;

namespace HealthChecks.Milvus.Tests.Functional;

public class milvus_healthcheck_should(MilvusContainerFixture milvusContainerFixture) : IClassFixture<MilvusContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_milvus_is_available_using_client_factory()
    {
        string connectionString = milvusContainerFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddMilvus(
                        clientFactory: sp => new MilvusClient(new Uri(connectionString)), tags: new string[] { "milvus" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("milvus")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_milvus_is_available_using_singleton()
    {
        string connectionString = milvusContainerFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(new MilvusClient(new Uri(connectionString)))
                    .AddHealthChecks()
                    .AddMilvus(tags: new string[] { "milvus" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("milvus")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_milvus_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddMilvus(
                        clientFactory: sp => new MilvusClient("255.255.255.255"), tags: new string[] { "milvus" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("milvus")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
