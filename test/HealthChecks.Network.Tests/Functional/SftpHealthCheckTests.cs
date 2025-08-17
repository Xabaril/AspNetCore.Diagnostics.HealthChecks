using System.Net;
using HealthChecks.Network.Tests.Fixtures;

namespace HealthChecks.Network.Tests.Functional;

public class sftp_healthcheck_should(SftpGoContainerFixture sftpGoFixture) : IClassFixture<SftpGoContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_connection_to_sftp_is_successful_using_password_authentication()
    {
        var properties = sftpGoFixture.GetSftpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSftpHealthCheck(setup =>
                {
                    var cfg = new SftpConfigurationBuilder(properties.Hostname, properties.Port, properties.Username)
                                    .AddPasswordAuthentication(properties.Password)
                                    .Build();

                    setup.AddHost(cfg);
                }, tags: ["sftp"], timeout: TimeSpan.FromSeconds(5));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sftp")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_connection_to_sftp_is_using_wrong_password()
    {
        var properties = sftpGoFixture.GetSftpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSftpHealthCheck(setup =>
                {
                    var cfg = new SftpConfigurationBuilder(properties.Hostname, properties.Port, properties.Username)
                                    .AddPasswordAuthentication("wrongpass")
                                    .Build();

                    setup.AddHost(cfg);
                }, tags: ["sftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sftp")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_when_connection_to_sftp_is_successful_using_private_key()
    {
        var properties = sftpGoFixture.GetSftpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSftpHealthCheck(setup =>
                {
                    var cfg = new SftpConfigurationBuilder(properties.Hostname, properties.Port, properties.Username)
                                    .AddPrivateKeyAuthentication(properties.PrivateKey, properties.Passphrase)
                                    .Build();

                    setup.AddHost(cfg);
                }, tags: ["sftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sftp")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_with_valid_authorization_and_file_creation_enabled()
    {
        var properties = sftpGoFixture.GetSftpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSftpHealthCheck(setup =>
                {
                    var cfg = new SftpConfigurationBuilder(properties.Hostname, properties.Port, properties.Username)
                                    .AddPrivateKeyAuthentication(properties.PrivateKey, properties.Passphrase)
                                    .CreateFileOnConnect("beatpulse")
                                    .Build();

                    setup.AddHost(cfg);
                }, tags: ["sftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sftp")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_with_one_valid_authorization()
    {
        var properties = sftpGoFixture.GetSftpConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSftpHealthCheck(setup =>
                {
                    var cfg = new SftpConfigurationBuilder(properties.Hostname, properties.Port, properties.Username)
                                    .AddPasswordAuthentication("wrongpass")
                                    .AddPrivateKeyAuthentication(properties.PrivateKey, properties.Passphrase)
                                    .Build();

                    setup.AddHost(cfg);
                }, tags: ["sftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sftp")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_using_wrong_port()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSftpHealthCheck(setup =>
                {
                    var cfg = new SftpConfigurationBuilder("localhost", 5551, "foo")
                                    .AddPasswordAuthentication("pass")
                                    .Build();

                    setup.AddHost(cfg);
                }, tags: ["sftp"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sftp")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
