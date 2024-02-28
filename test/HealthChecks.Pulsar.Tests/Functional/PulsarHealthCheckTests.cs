using System.Net;
using DotPulsar;

namespace HealthChecks.Pulsar.Tests.Functional;

public class pulsar_healthcheck_should
{
    [Fact]
    public async Task be_unhealthy_if_pulsar_is_unavailable()
    {
        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri("pulsar://localhost:1234"))
            .Build();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddPulsar(_ => client, tags: new string[] { "pulsar" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("pulsar")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_pulsar_is_available()
    {
        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri("pulsar://localhost:6650"))
            .Build();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddPulsar(_ => client, tags: new string[] { "pulsar" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("pulsar")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
