using System.Net;

namespace HealthChecks.Gremlin.Tests.Functional;

public class gremlin_healthcheck_should(GremlinContainerFixture gremlinFixture) : IClassFixture<GremlinContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_gremlin_is_available()
    {
        var options = gremlinFixture.GetConnectionOptions();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddGremlin(_ => options, tags: ["gremlin"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("gremlin")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_gremlin_are_available()
    {
        var options = gremlinFixture.GetConnectionOptions();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddGremlin(_ => options, tags: ["gremlin"], name: "1")
                 .AddGremlin(_ => options, tags: ["gremlin"], name: "2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("gremlin")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_gremlin_is_not_available()
    {
        var options = gremlinFixture.GetConnectionOptions();

        options.Hostname = "wronghost";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddGremlin(_ => options, tags: ["gremlin"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("gremlin")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
