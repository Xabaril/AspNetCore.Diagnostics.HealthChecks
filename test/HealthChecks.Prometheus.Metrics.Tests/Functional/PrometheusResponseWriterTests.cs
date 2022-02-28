using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace HealthChecks.Prometheus.Metrics.Tests.Functional
{
    public class prometheus_response_writer_should
    {
        [Fact]
        public async Task be_healthy_when_health_checks_are()
        {
            var sut = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Healthy());
                })
                .Configure(app => app.UseHealthChecksPrometheusExporter("/health")));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("fake", HealthStatus.Healthy);
        }

        [Fact]
        public async Task be_unhealthy_and_return_503_when_health_checks_are()
        {
            var sut = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app => app.UseHealthChecksPrometheusExporter("/health")));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("fake", HealthStatus.Unhealthy);
        }

        [Fact]
        public async Task be_unhealthy_and_return_configured_status_code_when_health_checks_are()
        {
            var sut = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app => app.UseHealthChecksPrometheusExporter("/health", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK)));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("fake", HealthStatus.Unhealthy);
        }
    }
}
