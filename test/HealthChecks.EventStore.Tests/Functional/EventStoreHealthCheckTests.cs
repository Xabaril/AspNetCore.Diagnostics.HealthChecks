using System.Net;
using EventStore.ClientAPI;
using HealthChecks.UI.Client;

namespace HealthChecks.EventStore.Tests.Functional
{
    public class eventstore_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_eventstore_is_available_using_eventstoreconnection()
        {
            var connectionSettingsBuilder = ConnectionSettings
                .Create()
                .SetCompatibilityMode("auto")
                .DisableTls();

            var connection = EventStoreConnection
                .Create(connectionSettingsBuilder, new IPEndPoint(IPAddress.Loopback, 1113));

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddEventStore(connection, tags: new string[] { "eventstore" });
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task be_healthy_if_eventstore_is_available_using_running_eventstoreconnection()
        {
            var connectionSettingsBuilder = ConnectionSettings
                .Create()
                .SetCompatibilityMode("auto")
                .DisableTls();

            var connection = EventStoreConnection
                .Create(connectionSettingsBuilder, new IPEndPoint(IPAddress.Loopback, 1113));

            await connection.ConnectAsync();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddEventStore(connection, tags: new[] { "eventstore" });
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }


        [Fact]
        public async Task be_healthy_if_eventstore_is_available_using_eventstoreconnection_with_several_checks()
        {
            var connectionSettingsBuilder = ConnectionSettings
                .Create()
                .SetCompatibilityMode("auto")
                .DisableTls();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddEventStore(EventStoreConnection.Create(connectionSettingsBuilder, new IPEndPoint(IPAddress.Loopback, 1113)),
                            tags: new[]
                            {
                                "eventstore"
                            });
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            var secondResponse = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

            secondResponse.StatusCode
                .Should().Be(HttpStatusCode.OK, await secondResponse.Content.ReadAsStringAsync());

        }

        [Fact]
        public async Task be_healthy_if_eventstore_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddEventStore("ConnectTo=tcp://localhost:1113; UseSslConnection=false; HeartBeatTimeout=500", tags: new string[] { "eventstore" });
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }


        [Fact]
        public async Task be_healthy_if_eventstore_is_available_with_several_checks()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddEventStore("ConnectTo=tcp://localhost:1113; UseSslConnection=false; HeartBeatTimeout=500", tags: new string[] { "eventstore" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health",
                        new HealthCheckOptions
                        {
                            Predicate = r => r.Tags.Contains("eventstore"),
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                        });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            var secondResponse = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

            secondResponse.StatusCode
                .Should().Be(HttpStatusCode.OK, await secondResponse.Content.ReadAsStringAsync());

        }

        [Fact]
        public async Task be_unhealthy_if_eventstore_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    // Wrong port, correct hostname - Inaccessible hostname results in a timeout
                    services.AddHealthChecks()
                        .AddEventStore("ConnectTo=tcp://localhost:1114; UseSslConnection=false; HeartBeatTimeout=500", tags: new string[] { "eventstore" });
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

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
        }
    }
}
