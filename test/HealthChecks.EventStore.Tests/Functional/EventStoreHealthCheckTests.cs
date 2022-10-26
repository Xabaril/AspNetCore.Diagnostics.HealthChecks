using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.EventStore.Tests.Functional
{
    public class eventstore_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_eventstore_is_available_with_uri_format()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<HealthCheckOptions>(opt => opt.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse);
                    services.AddHealthChecks()
                        .AddEventStore("ConnectTo=tcp://localhost:1113; UseSslConnection=false", tags: new string[] { "eventstore" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("eventstore")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task be_healthy_if_eventstore_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<HealthCheckOptions>(opt => opt.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse);
                    services.AddHealthChecks()
                        .AddEventStore("ConnectTo=tcp://localhost:1113; UseSslConnection=false; HeartBeatTimeout=500", tags: new string[] { "eventstore" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("eventstore")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task be_unhealthy_if_eventstore_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<HealthCheckOptions>(opt => opt.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse);
                    services.AddHealthChecks()
                    .AddEventStore("tcp://nonexistingdomain:1113", tags: new string[] { "eventstore" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("eventstore")
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
