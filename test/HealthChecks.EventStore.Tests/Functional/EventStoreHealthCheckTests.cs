using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.EventStore.Tests.Functional;

public class eventstore_healthcheck_should(EventStoreDbContainerFixture eventStoreDbFixture) : IClassFixture<EventStoreDbContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_eventstore_is_available_with_uri_format()
    {
        string connectionString = eventStoreDbFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEventStore(connectionString, tags: ["eventstore"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("eventstore"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_healthy_if_eventstore_is_available()
    {
        string connectionString = eventStoreDbFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEventStore(connectionString, tags: ["eventstore"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("eventstore"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_eventstore_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                // Existing hostname, incorrect port. If the hostname cannot be reached, CreateRequest will hang.
                services.AddHealthChecks()
                .AddEventStore("ConnectTo=tcp://localhost:1114; UseSslConnection=false; HeartBeatTimeout=500", tags: ["eventstore"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("eventstore"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }
}
