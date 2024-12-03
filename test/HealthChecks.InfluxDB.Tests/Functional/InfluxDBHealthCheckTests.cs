using System.Net;

namespace HealthChecks.InfluxDB.Tests.Functional;

public class influxdb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_influxdb_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user", "password", "influxdb", tags: ["influxdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("influxdb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_influxdb_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user_unavailable", "password", "influxdb", tags: ["influxdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("influxdb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
