using System.Net;

namespace HealthChecks.Network.Tests.Functional;

public class ping_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_all_hosts_reply_to_ping()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddPingHealthCheck(setup => setup.AddHost("127.0.0.1", 5000), tags: ["ping"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ping")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_a_host_ping_is_not_successful()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddPingHealthCheck(setup =>
                    {
                        setup.AddHost("127.0.0.1", 3000);
                        setup.AddHost("nonexistinghost", 3000);
                    }, tags: ["ping"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ping")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
