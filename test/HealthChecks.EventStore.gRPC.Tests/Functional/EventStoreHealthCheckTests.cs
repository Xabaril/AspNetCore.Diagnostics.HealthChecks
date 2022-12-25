using System.Net;

namespace HealthChecks.EventStore.gRPC.Tests.Functional;

public class eventstore_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_eventstore_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEventStore("esdb://localhost:2113?tls=false", tags: new[] { "eventstore" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("eventstore")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_eventstore_tls_settings_do_not_match()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEventStore("esdb://localhost:2113", tags: new[] { "eventstore" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("eventstore")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_eventstore_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEventStore("esdb://nonexistingdomain:2113?tls=false", tags: new[] { "eventstore" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("eventstore")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
