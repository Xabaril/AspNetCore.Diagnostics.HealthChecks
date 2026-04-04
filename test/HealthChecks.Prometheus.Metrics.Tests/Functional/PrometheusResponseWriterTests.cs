using System.Net;

namespace HealthChecks.Prometheus.Metrics.Tests.Functional;

public class prometheus_response_writer_should
{
    [Fact]
    public async Task be_healthy_when_health_checks_are()
    {
        var appBuilder = WebApplication.CreateBuilder();
        appBuilder.Services.AddHealthChecks()
            .AddCheck("fake", check => HealthCheckResult.Healthy());
        appBuilder.WebHost.UseTestServer();
        await using var sut = appBuilder.Build();
        sut.UseHealthChecksPrometheusExporter("/health");
        await sut.StartAsync();

        using var response = await sut.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var resultAsString = await response.Content.ReadAsStringAsync();
        resultAsString.ShouldContainCheckAndResult("fake", HealthStatus.Healthy);
    }

    [Fact]
    public async Task be_unhealthy_and_return_503_when_health_checks_are()
    {
        var appBuilder = WebApplication.CreateBuilder();
        appBuilder.Services.AddHealthChecks()
            .AddCheck("fake", check => HealthCheckResult.Unhealthy());
        appBuilder.WebHost.UseTestServer();
        await using var sut = appBuilder.Build();
        sut.UseHealthChecksPrometheusExporter("/health");
        await sut.StartAsync();

        using var response = await sut.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var resultAsString = await response.Content.ReadAsStringAsync();
        resultAsString.ShouldContainCheckAndResult("fake", HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task be_unhealthy_and_return_configured_status_code_when_health_checks_are()
    {
        var appBuilder = WebApplication.CreateBuilder();
        appBuilder.Services.AddHealthChecks()
            .AddCheck("fake", check => HealthCheckResult.Unhealthy());
        appBuilder.WebHost.UseTestServer();
        await using var sut = appBuilder.Build();
        sut.UseHealthChecksPrometheusExporter("/health", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK);
        await sut.StartAsync();

        using var response = await sut.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var resultAsString = await response.Content.ReadAsStringAsync();
        resultAsString.ShouldContainCheckAndResult("fake", HealthStatus.Unhealthy);
    }
}
