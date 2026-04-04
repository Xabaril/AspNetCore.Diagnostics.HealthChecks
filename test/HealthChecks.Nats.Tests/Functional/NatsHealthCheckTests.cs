using System.Net;
using NATS.Client.Core;
using NATS.Client.Hosting;
using static HealthChecks.Nats.Tests.Defines;

namespace HealthChecks.Nats.Tests.Functional;

public class nats_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_nats_is_available_using_client_factory()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    var options = NatsOpts.Default with
                    {
                        Url = DefaultLocalConnectionString,
                    };
                    var natsConnection = new NatsConnection(options);

                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddNats(
                            clientFactory: sp => natsConnection, tags: new string[] { "nats" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("nats")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_nats_is_available_using_singleton()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    var options = NatsOpts.Default with
                    {
                        Url = DefaultLocalConnectionString,
                    };

                    webHostBuilder.Services
                        .AddSingleton<INatsConnection>(new NatsConnection(options))
                        .AddHealthChecks()
                        .AddNats(tags: new string[] { "nats" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("nats")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_nats_is_unavailable()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddNats(
                            clientFactory: sp =>
                            {
                                var options = NatsOpts.Default with
                                {
                                    Url = ConnectionStringDoesNotExistOrStopped,
                                };
                                return new NatsConnection(options);
                            }, tags: new string[] { "nats" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("nats")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
