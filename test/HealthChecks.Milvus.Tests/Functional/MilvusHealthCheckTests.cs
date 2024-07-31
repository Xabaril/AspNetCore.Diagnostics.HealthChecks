using System.Net;
using Milvus.Client;

namespace HealthChecks.Milvus.Tests.Functional;

public class milvus_healthcheck_should
{
    [Fact(Skip = "Doesn't work at the moment")]
    public async Task be_healthy_when_milvus_is_available_using_client_factory()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddMilvus(
                        clientFactory: sp => new MilvusClient("localhost"), tags: new string[] { "milvus" });
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

    [Fact(Skip = "Doesn't work at the moment")]
    public async Task be_healthy_when_milvus_is_available_using_singleton()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(new MilvusClient("localhost"))
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

    [Fact(Skip = "Doesn't work at the moment")]
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
