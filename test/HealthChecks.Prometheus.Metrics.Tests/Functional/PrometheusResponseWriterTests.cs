using System.Net;

namespace HealthChecks.Prometheus.Metrics.Tests.Functional;

public class prometheus_response_writer_should
{
    [Fact]
    public async Task be_healthy_when_health_checks_are()
    {
        using var sut = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCheck("fake", check => HealthCheckResult.Healthy());
            })
            .Configure(app => app.UseHealthChecksPrometheusExporter("/health")));

        using var response = await sut.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        var resultAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        resultAsString.ShouldContainCheckAndResult("fake", HealthStatus.Healthy);
    }

    [Fact]
    public async Task be_unhealthy_and_return_503_when_health_checks_are()
    {
        using var sut = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCheck("fake", check => HealthCheckResult.Unhealthy());
            })
            .Configure(app => app.UseHealthChecksPrometheusExporter("/health")));

        using var response = await sut.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var resultAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        resultAsString.ShouldContainCheckAndResult("fake", HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task be_unhealthy_and_return_configured_status_code_when_health_checks_are()
    {
        using var sut = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCheck("fake", check => HealthCheckResult.Unhealthy());
            })
            .Configure(app => app.UseHealthChecksPrometheusExporter("/health", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK)));

        using var response = await sut.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var resultAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        resultAsString.ShouldContainCheckAndResult("fake", HealthStatus.Unhealthy);
    }
}
