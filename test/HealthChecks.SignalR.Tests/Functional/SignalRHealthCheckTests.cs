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
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddSignalR()
                .Services
                .AddHealthChecks()
                .AddSignalRHub(
                    () => new HubConnectionBuilder()
                            .WithUrl("http://localhost/test", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                            .Build(),
                    tags: new string[] { "signalr" });
            })
            .Configure(app =>
            {

                app
                    .UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("signalr")
                    })
                    .UseRouting()
                    .UseEndpoints(config => config.MapHub<TestHub>("/test"));
            });

        server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        server.Dispose();
    }

    [Fact]
    public async Task be_unhealthy_if_signalr_hub_is_unavailable()
    {
        TestServer server = null!;
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddSignalR()
                .Services
                .AddHealthChecks()
                .AddSignalRHub(
                    () => new HubConnectionBuilder()
                            .WithUrl("http://localhost/badhub", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                            .Build(),
                    tags: new string[] { "signalr" });
            })
            .Configure(app =>
            {
                app
                    .UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("signalr")
                    })
                    .UseRouting()
                    .UseEndpoints(config => config.MapHub<TestHub>("/test"));
            });

        server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);

        server.Dispose();
    }

    private class TestHub : Hub
    {

    }
}
