using System.Net;

namespace HealthChecks.Network.Tests.Functional
{
    public class sftp_healthcheck_should
    {

        [Fact]
        public async Task be_healthy_when_connection_to_sftp_is_successful_using_password_authentication()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPasswordAuthentication("pass")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: new string[] { "sftp" }, timeout: TimeSpan.FromSeconds(5));
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_unhealthy_when_connection_to_sftp_is_using_wrong_password()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPasswordAuthentication("wrongpass")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: new string[] { "sftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_when_connection_to_sftp_is_successful_using_private_key()
        {
            string privateKey = File.ReadAllText("id_rsa");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPrivateKeyAuthentication(privateKey, "beatpulse")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: new string[] { "sftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_healthy_with_valid_authorization_and_file_creation_enabled()
        {
            string privateKey = File.ReadAllText("id_rsa");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPrivateKeyAuthentication(privateKey, "beatpulse")
                                        .CreateFileOnConnect("upload/beatpulse")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: new string[] { "sftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_healthy_with_one_valid_authorization()
        {
            string privateKey = File.ReadAllText("id_rsa");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSftpHealthCheck(setup =>
                    {
                        var cfg = new SftpConfigurationBuilder("localhost", 22, "foo")
                                        .AddPasswordAuthentication("wrongpass")
                                        .AddPrivateKeyAuthentication(privateKey, "beatpulse")
                                        .Build();

                        setup.AddHost(cfg);
                    }, tags: new string[] { "sftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

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
                    }, tags: new string[] { "sftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
