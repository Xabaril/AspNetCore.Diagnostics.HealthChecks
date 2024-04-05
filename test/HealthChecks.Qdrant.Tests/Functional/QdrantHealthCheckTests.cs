using System.Net;
using Qdrant.Client;

namespace HealthChecks.Qdrant.Tests.Functional;

public class qdrant_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_qdrant_is_available()
    {
        var connectionString = "http://localhost:6334";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddQdrant(qdrantConnectionString: connectionString, tags: new string[] { "qdrant" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("qdrant")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_qdrant_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddQdrant(qdrantConnectionString: "http://localhost:6334", tags: new string[] { "qdrant" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("qdrant")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_qdrant_is_available_using_client()
    {
        var connectionString = "http://localhost:6334";

        var client = new QdrantClient(address: new Uri(connectionString));

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<QdrantClient>(client)
                    .AddHealthChecks()
                    .AddQdrant(tags: new string[] { "qdrant" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("qdrant")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_qdrant_is_available_using_iServiceProvider()
    {
        var connectionString = "http://localhost:6334";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddQdrant(setup: options => options.ConnectionUri = new Uri(connectionString), tags: new string[] { "qdrant" });

            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("qdrant")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
