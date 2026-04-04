using System.Net;

namespace HealthChecks.Network.Tests.Functional;

public class sftp_healthcheck_should
{

    [Fact]
    public async Task be_healthy_when_connection_to_sftp_is_successful_using_password_authentication()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPasswordAuthentication("pass")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: ["sftp"], timeout: TimeSpan.FromSeconds(5));
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_connection_to_sftp_is_using_wrong_password()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPasswordAuthentication("wrongpass")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: ["sftp"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_when_connection_to_sftp_is_successful_using_private_key()
    {
        string privateKey = File.ReadAllText("id_rsa");

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPrivateKeyAuthentication(privateKey, "beatpulse")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: ["sftp"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_with_valid_authorization_and_file_creation_enabled()
    {
        string privateKey = File.ReadAllText("id_rsa");

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPrivateKeyAuthentication(privateKey, "beatpulse")
                                        .CreateFileOnConnect("upload/beatpulse")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: ["sftp"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_healthy_with_one_valid_authorization()
    {
        string privateKey = File.ReadAllText("id_rsa");

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPasswordAuthentication("wrongpass")
                                        .AddPrivateKeyAuthentication(privateKey, "beatpulse")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: ["sftp"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_using_wrong_port()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 5551, "foo")
                                        .AddPasswordAuthentication("pass")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: ["sftp"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
