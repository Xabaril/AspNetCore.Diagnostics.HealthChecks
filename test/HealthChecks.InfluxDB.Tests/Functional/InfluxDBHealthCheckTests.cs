using System.Net;

namespace HealthChecks.InfluxDB.Tests.Functional;

public class influxdb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_influxdb_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddHealthChecks()
                    .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user", "password", "influxdb", tags: ["influxdb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("influxdb")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_influxdb_is_unavailable()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user_unavailable", "password", "influxdb", tags: ["influxdb"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("influxdb")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
