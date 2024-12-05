using System.Net;

namespace HealthChecks.Network.Tests.Functional;

public class ssl_healthcheck_should
{
    //Use https://badssl.com web site, witch provide samples certificates with various states and types
    private const string _validHost256 = "sha256.badssl.com";
    private const string _validHost384 = "sha384.badssl.com";
    private const string _validHost512 = "sha512.badssl.com";
    private const string _httpHost = "http.badssl.com";
    private const string _revokedHost = "revoked.badssl.com";
    private const string _expiredHost = "expired.badssl.com";

    [Fact]
    public async Task be_healthy_if_ssl_is_valid()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSslHealthCheck(options =>
                {
                    options.AddHost(_validHost256);
                    options.AddHost(_validHost384);
                    options.AddHost(_validHost512);
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
                .AddSslHealthCheck(options => options.AddHost(_httpHost, 80), tags: ["ssl"]);
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
                .AddSslHealthCheck(options => options.AddHost(_revokedHost), tags: ["ssl"]);
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
                .AddSslHealthCheck(options => options.AddHost(_expiredHost), tags: ["ssl"]);
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
                .AddSslHealthCheck(options => options.AddHost(_validHost256, checkLeftDays: 1095), tags: ["ssl"]);
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
        options.AddHost("invalid", 5555);

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
