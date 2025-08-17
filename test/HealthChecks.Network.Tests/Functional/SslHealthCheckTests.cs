using System.Net;

namespace HealthChecks.Network.Tests.Functional;

public class ssl_healthcheck_should
{
    private const string ValidHost = "microsoft.com";

    // TODO: BadSSL is archived, tests might break in the future: https://github.com/chromium/badssl.com
    private const string HttpHost = "http.badssl.com";
    private const string RevokedHost = "revoked.badssl.com";
    private const string ExpiredHost = "expired.badssl.com";

    [Fact]
    public async Task be_healthy_if_ssl_is_valid()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSslHealthCheck(options =>
                {
                    options.AddHost(ValidHost, checkLeftDays: 0);
                }, tags: ["ssl"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ssl")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_ssl_is_not_present()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSslHealthCheck(options => options.AddHost(HttpHost, 80), tags: ["ssl"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ssl")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_ssl_is_not_valid()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSslHealthCheck(options => options.AddHost(RevokedHost), tags: ["ssl"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ssl")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_ssl_is_expired()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSslHealthCheck(options => options.AddHost(ExpiredHost), tags: ["ssl"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ssl")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_degraded_if_ssl_daysbefore()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSslHealthCheck(options => options.AddHost(ValidHost, checkLeftDays: 1095), tags: ["ssl"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ssl")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var resultAsString = await response.Content.ReadAsStringAsync();
        resultAsString.ShouldContain(HealthStatus.Unhealthy.ToString());
    }

    [Fact]
    public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
    {
        var options = new SslHealthCheckOptions();
        options.AddHost("10.255.255.255", 5555);

        var sslHealthCheck = new SslHealthCheck(options);

        var result = await sslHealthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "ssl",
                instance: sslHealthCheck,
                failureStatus: HealthStatus.Degraded,
                null,
                timeout: null)
        }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

        result.Exception.ShouldBeOfType<OperationCanceledException>();
    }
}
