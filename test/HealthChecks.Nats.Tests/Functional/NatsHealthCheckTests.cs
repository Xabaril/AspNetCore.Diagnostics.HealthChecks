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
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var options = NatsOpts.Default with
                {
                    Url = DefaultLocalConnectionString,
                };
                var natsConnection = new NatsConnection(options);

                services
                    .AddHealthChecks()
                    .AddNats(
                        clientFactory: sp => natsConnection, tags: new string[] { "nats" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("nats")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_nats_is_available_using_singleton()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var options = NatsOpts.Default with
                {
                    Url = DefaultLocalConnectionString,
                };

                services
                    .AddSingleton<INatsConnection>(new NatsConnection(options))
                    .AddHealthChecks()
                    .AddNats(tags: new string[] { "nats" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("nats")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_nats_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
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
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("nats")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
