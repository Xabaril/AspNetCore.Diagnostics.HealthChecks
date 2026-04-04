using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace HealthChecks.SignalR.Tests.Functional;

public class signalr_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_signalr_hub_is_available()
    {
        TestServer server = null!;
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddSignalR()
                    .Services
                    .AddHealthChecks()
                    .AddSignalRHub(
                        () => new HubConnectionBuilder()
                                .WithUrl("http://localhost/test", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                                .Build(),
                        tags: ["signalr"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app
                        .UseHealthChecks("/health", new HealthCheckOptions
                        {
                            Predicate = r => r.Tags.Contains("signalr")
                        })
                        .UseRouting()
                        .UseEndpoints(config => config.MapHub<TestHub>("/test"));
        await app.StartAsync();

        server = app.GetTestServer();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);


    }

    [Fact]
    public async Task be_unhealthy_if_signalr_hub_is_unavailable()
    {
        TestServer server = null!;
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddSignalR()
                    .Services
                    .AddHealthChecks()
                    .AddSignalRHub(
                        () => new HubConnectionBuilder()
                                .WithUrl("http://localhost/badhub", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                                .Build(),
                        tags: ["signalr"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app
                        .UseHealthChecks("/health", new HealthCheckOptions
                        {
                            Predicate = r => r.Tags.Contains("signalr")
                        })
                        .UseRouting()
                        .UseEndpoints(config => config.MapHub<TestHub>("/test"));
        await app.StartAsync();

        server = app.GetTestServer();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);


    }

    private class TestHub : Hub
    {

    }
}
