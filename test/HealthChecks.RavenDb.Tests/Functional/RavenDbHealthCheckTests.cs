using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.RavenDb.Tests.Functional;

public class ravendb_healthcheck_should(RavenDbContainerFixture ravenDbFixture) : IClassFixture<RavenDbContainerFixture>
{
    private readonly string[] _urls = [ravenDbFixture.GetConnectionString()];

    [Fact]
    public async Task be_healthy_if_ravendb_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddRavenDB(_ => _.Urls = _urls, tags: ["ravendb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ravendb"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddRavenDB(_ =>
                        {
                            _.Urls = _urls;
                            _.Database = "Demo";
                        }, tags: ["ravendb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ravendb"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_timeout_is_too_low()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddRavenDB(_ =>
                        {
                            _.Urls = _urls;
                            _.Database = "Demo";
                            _.RequestTimeout = TimeSpan.FromMilliseconds(0.001);
                        }, tags: ["ravendb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ravendb"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_not_available()
    {
        var connectionString = "http://localhost:9999";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddRavenDB(_ => _.Urls = [connectionString], tags: ["ravendb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ravendb"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_database_doesnot_exist()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddRavenDB(_ =>
                        {
                            _.Urls = _urls;
                            _.Database = "ThisDatabaseReallyDoesnExist";
                        }, tags: ["ravendb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ravendb"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }
}
