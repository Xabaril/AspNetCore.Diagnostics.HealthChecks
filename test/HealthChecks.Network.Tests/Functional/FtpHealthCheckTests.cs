using System.Net;
using Microsoft.AspNetCore.Http;

namespace HealthChecks.Network.Tests.Functional;

public class ftp_healthcheck_should
{

    [Fact]
    public async Task be_healthy_when_connection_is_successful()
    {
        var webHostBuilder = new WebHostBuilder()
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
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ftp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe((HttpStatusCode)StatusCodes.Status200OK);
    }

    [Fact]
    public async Task be_healthy_when_connection_is_successful_and_file_is_created()
    {
        var webHostBuilder = new WebHostBuilder()
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
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ftp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

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
            Registration = new HealthCheckRegistration(
                "ftp",
                instance: ftpHealthCheck,
                failureStatus: HealthStatus.Degraded,
                null,
                timeout: null)
        }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token).ConfigureAwait(false);

        result.Exception.ShouldBeOfType<OperationCanceledException>();
    }
}
