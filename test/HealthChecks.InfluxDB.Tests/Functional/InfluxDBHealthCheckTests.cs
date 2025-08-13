using System.Net;

namespace HealthChecks.InfluxDB.Tests.Functional;

public class influxdb_healthcheck_should(InfluxDBContainerFixture influxDBFixture) : IClassFixture<InfluxDBContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_influxdb_is_available()
    {
        var properties = influxDBFixture.GetConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddInfluxDB(properties.Address, properties.Username, properties.Password, "influxdb", tags: ["influxdb"]);
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
        var properties = influxDBFixture.GetConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddInfluxDB(properties.Address, "invalid_user", properties.Password, "influxdb", tags: ["influxdb"]);
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
