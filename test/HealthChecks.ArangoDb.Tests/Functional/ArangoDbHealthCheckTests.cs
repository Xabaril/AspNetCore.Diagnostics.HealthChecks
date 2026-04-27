using System.Net;

namespace HealthChecks.ArangoDb.Tests.Functional;

public class arangodb_healthcheck_should(ArangoDbContainerFixture arangoDbFixture) : IClassFixture<ArangoDbContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_arangodb_is_available()
    {
        var options = arangoDbFixture.GetConnectionOptions();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddArangoDb(_ => options, tags: ["arangodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("arangodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_arango_are_available()
    {
        var options = arangoDbFixture.GetConnectionOptions();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddArangoDb(_ => options, tags: ["arango"], name: "1")
                    .AddArangoDb(_ => options, tags: ["arango"], name: "2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("arango")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_arango_is_not_available()
    {
        var options = arangoDbFixture.GetConnectionOptions();

        options.Password = "invalid password";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddArangoDb(_ => options, tags: ["arango"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("arango")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
