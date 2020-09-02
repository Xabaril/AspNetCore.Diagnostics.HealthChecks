using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Raven.Client.Documents.Operations;
using Xunit;

namespace FunctionalTests.HealthChecks.Network
{
    [Collection("execution")]
    public class ftp_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public ftp_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_connection_is_successful()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddFtpHealthCheck(setup =>
                        {
                            setup.AddHost("ftp://localhost:21",
                                createFile: false,
                                credentials: new NetworkCredential("bob", "12345"));
                        }, tags: new string[] { "ftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("ftp")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_connection_is_successful_and_file_is_created()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddFtpHealthCheck(setup =>
                        {
                            setup.AddHost("ftp://localhost:21",
                                createFile: true,
                                credentials: new NetworkCredential("bob", "12345"));
                        }, tags: new string[] { "ftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("ftp")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
        {
            var options = new FtpHealthCheckOptions();
            options.AddHost("ftp://invalid:21");
            var ftpHealthCheck = new FtpHealthCheck(options);

            var result = await ftpHealthCheck.CheckHealthAsync(new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("ftp", instance: ftpHealthCheck, failureStatus: HealthStatus.Degraded,
                    null, timeout: null)
            }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

            result.Exception.Should().BeOfType<OperationCanceledException>();
        }
    }
}