using System.Net;
using NATS.Client.Core;

namespace HealthChecks.Nats.Tests.Functional;

public class nats_healthcheck_should(NatsContainerFixture natsFixture) : IClassFixture<NatsContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_nats_is_available_using_client_factory()
    {
        string connectionString = natsFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var options = NatsOpts.Default with
                {
                    Url = connectionString,
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
        string connectionString = natsFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var options = NatsOpts.Default with
                {
                    Url = connectionString
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
                                Url = "nats://DoesNotExist:4222",
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
