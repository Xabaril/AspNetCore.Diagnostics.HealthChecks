using System.Net;
using HealthChecks.Network.Core;
using HealthChecks.Network.Tests.Fixtures;

namespace HealthChecks.Network.Tests.Functional;

public class smtp_healthcheck_should(SecureDockerMailServerContainerFixture dockerMailServerFixture) : IClassFixture<SecureDockerMailServerContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_connecting_using_ssl()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSmtpHealthCheck(setup =>
                {
                    //SSL on by default
                    setup.Host = properties.Host;
                    setup.Port = properties.ImplicitTlsPort;
                    setup.ConnectionType = SmtpConnectionType.SSL;
                    setup.AllowInvalidRemoteCertificates = true;
                }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_when_connecting_using_tls()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSmtpHealthCheck(setup =>
                {
                    //SSL on by default
                    setup.Host = properties.Host;
                    setup.Port = properties.ExplicitTlsPort;
                    setup.ConnectionType = SmtpConnectionType.TLS;
                    setup.AllowInvalidRemoteCertificates = true;
                }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_when_connecting_using_connection_type_tls()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSmtpHealthCheck(setup =>
                {
                    setup.Host = properties.Host;
                    setup.Port = properties.ExplicitTlsPort;
                    setup.ConnectionType = SmtpConnectionType.TLS;
                    setup.AllowInvalidRemoteCertificates = true;
                }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_connecting_to_an_invalid_smtp_port_with_mode_auto()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        setup.Host = properties.Host;
                        setup.Port = 45;
                        setup.AllowInvalidRemoteCertificates = true;
                    }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_when_connection_and_login_with_valid_account_using_ssl()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSmtpHealthCheck(setup =>
                {
                    setup.Host = properties.Host;
                    setup.Port = dockerMailServerFixture.Container!.GetMappedPublicPort(465);
                    setup.AllowInvalidRemoteCertificates = true;
                    setup.ConnectionType = SmtpConnectionType.SSL;
                    setup.LoginWith(properties.Username, properties.Password);
                }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_when_connection_and_login_with_valid_account_using_tls()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSmtpHealthCheck(setup =>
                {
                    setup.Host = properties.Host;
                    setup.Port = properties.ExplicitTlsPort;
                    setup.AllowInvalidRemoteCertificates = true;
                    setup.ConnectionType = SmtpConnectionType.TLS;
                    setup.LoginWith(properties.Username, properties.Password);
                }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();

    }

    [Fact]
    public async Task be_unhealthy_when_connection_and_login_with_an_invalid_account()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddSmtpHealthCheck(setup =>
                  {
                      setup.Host = properties.Host;
                      setup.Port = properties.ExplicitTlsPort;
                      setup.ConnectionType = SmtpConnectionType.TLS;
                      setup.AllowInvalidRemoteCertificates = true;
                      setup.LoginWith("admin@healthchecks.com", "wrongpass");
                  }, tags: ["smtp"]);
             })
             .Configure(app =>
             {
                 app.UseHealthChecks("/health", new HealthCheckOptions
                 {
                     Predicate = r => r.Tags.Contains("smtp")
                 });
             });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
    {
        var options = new SmtpHealthCheckOptions() { Host = "10.255.255.255", Port = 25 };
        options.LoginWith("user", "pass");

        var smtpHealthCheck = new SmtpHealthCheck(options);

        var result = await smtpHealthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "smtp",
                instance: smtpHealthCheck,
                failureStatus: HealthStatus.Degraded,
                null,
                timeout: null)
        }, new CancellationTokenSource(TimeSpan.FromMilliseconds(100)).Token);

        result.Exception.ShouldBeOfType<OperationCanceledException>();
    }
}

public class plaintext_smtp_healthcheck_should(DockerMailServerContainerFixture dockerMailServerFixture) : IClassFixture<DockerMailServerContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_login_with_plain_smtp()
    {
        var properties = dockerMailServerFixture.GetSmtpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        setup.Host = properties.Host;
                        setup.Port = properties.ExplicitTlsPort;
                        setup.ConnectionType = SmtpConnectionType.PLAIN;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith(properties.Username, properties.Password);
                    }, tags: ["smtp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("smtp")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }
}
