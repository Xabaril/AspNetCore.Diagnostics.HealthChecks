using System.Net;
using HealthChecks.Network.Tests.Fixtures;
using Microsoft.AspNetCore.Http;

namespace HealthChecks.Network.Tests.Functional;

public class ftp_healthcheck_should(SftpGoContainerFixture sftpGoFixture) : IClassFixture<SftpGoContainerFixture>
{

    [Fact]
    public async Task be_healthy_when_connection_is_successful()
    {
        var properties = sftpGoFixture.GetFtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFtpHealthCheck(setup =>
                    {
                        setup.AddHost($"ftp://{properties.Hostname}:{properties.Port}",
                            createFile: false,
                            credentials: new NetworkCredential(properties.Username, properties.Password));
                    }, tags: ["ftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ftp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe((HttpStatusCode)StatusCodes.Status200OK);
    }

    [Fact]
    public async Task be_healthy_when_connection_is_successful_and_file_is_created()
    {
        var properties = sftpGoFixture.GetFtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFtpHealthCheck(setup =>
                    {
                        setup.AddHost($"ftp://{properties.Hostname}:{properties.Port}",
                            createFile: true,
                            credentials: new NetworkCredential(properties.Username, properties.Password));
                    }, tags: ["ftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ftp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
    {
        var options = new FtpHealthCheckOptions();
        options.AddHost("ftp://10.255.255.255:21");
        var ftpHealthCheck = new FtpHealthCheck(options);

        var result = await ftpHealthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "ftp",
                instance: ftpHealthCheck,
                failureStatus: HealthStatus.Degraded,
                null,
                timeout: null)
        }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

        result.Exception.ShouldBeOfType<OperationCanceledException>();
    }
}
