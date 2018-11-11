using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.Publisher.Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace FunctionalTests.HealthChecks.Publisher.Prometheus
{
    [Collection("execution")]
    public class prometheus_responsewriter_should
    {
        private readonly ExecutionFixture _fixture;

        public prometheus_responsewriter_should(ExecutionFixture fixture)
        {
            _fixture = fixture;
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_health_checks_are()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Healthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = (context, report) =>
                            PrometheusResponseWriter.WritePrometheusResultText(context, report)
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
            var resultAsString = await response.Content.ReadAsStringAsync();
            ShouldContainCheckAndResult(resultAsString,"fake",HealthStatus.Healthy.ToString("D"));
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_and_return_503_when_health_checks_are()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = (context, report) =>
                            PrometheusResponseWriter.WritePrometheusResultText(context, report)
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            var resultAsString = await response.Content.ReadAsStringAsync();
            ShouldContainCheckAndResult(resultAsString,"fake",HealthStatus.Unhealthy.ToString("D"));
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_and_return_200_when_health_checks_are_and_configured_to_return_always_200()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = (context, report) =>
                            PrometheusResponseWriter.WritePrometheusResultText(context, report,alwaysReturnHttp200Ok:true)
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resultAsString = await response.Content.ReadAsStringAsync();
            ShouldContainCheckAndResult(resultAsString,"fake",HealthStatus.Unhealthy.ToString("D"));
        }

        [SkipOnAppVeyor]
        public async Task have_all_results_included_when_there_are_three_checks_with_different_results()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("healthy_check", check => HealthCheckResult.Healthy())
                        .AddCheck("degraded_check", check => HealthCheckResult.Degraded())
                        .AddCheck("unhealthy_check", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = (context, report) =>
                            PrometheusResponseWriter.WritePrometheusResultText(context, report,alwaysReturnHttp200Ok:true)
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resultAsString = await response.Content.ReadAsStringAsync();

            ShouldContainCheckAndResult(resultAsString,"healthy_check", HealthStatus.Healthy.ToString("D"));
            ShouldContainCheckAndResult(resultAsString,"degraded_check", HealthStatus.Degraded.ToString("D"));
            ShouldContainCheckAndResult(resultAsString,"unhealthy_check", HealthStatus.Unhealthy.ToString("D"));
          
        }

        private void ShouldContainCheckAndResult(string resultAsString, string checkName, string expectedResult)
        {
            resultAsString.Should()
                .Contain($"liveness{{healthcheck=\"{checkName}\"}} {expectedResult}", "health check liveness must be included");
            resultAsString.Should().Contain($"liveness_duration_seconds{{healthcheck=\"{checkName}\"}}",
                "health check duration must be included");
        }
    }
}