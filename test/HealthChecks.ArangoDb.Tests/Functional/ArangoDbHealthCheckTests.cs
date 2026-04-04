using System.Net;

namespace HealthChecks.ArangoDb.Tests.Functional;

public class arangodb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_arangodb_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                     .AddArangoDb(_ => new ArangoDbOptions
                     {
                         HostUri = "http://localhost:8529/",
                         Database = "_system",
                         UserName = "root",
                         Password = "strongArangoDbPassword"
                     }, tags: ["arangodb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("arangodb")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_arango_are_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddArangoDb(_ => new ArangoDbOptions
                        {
                            HostUri = "http://localhost:8529/",
                            Database = "_system",
                            UserName = "root",
                            Password = "strongArangoDbPassword"
                        }, tags: ["arango"], name: "1")
                        .AddArangoDb(_ => new ArangoDbOptions
                        {
                            HostUri = "http://localhost:8529/",
                            Database = "_system",
                            UserName = "root",
                            Password = "strongArangoDbPassword",
                            IsGenerateJwtTokenBasedOnUserNameAndPassword = true
                        }, tags: ["arango"], name: "2");
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("arango")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_arango_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                     .AddArangoDb(_ => new ArangoDbOptions
                     {
                         HostUri = "http://localhost:8529/",
                         Database = "_system",
                         UserName = "root",
                         Password = "invalid password"
                     }, tags: ["arango"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("arango")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
