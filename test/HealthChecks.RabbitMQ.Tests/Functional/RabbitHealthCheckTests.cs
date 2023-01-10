using System.Net;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ.Tests.Functional
{
    public class rabbitmq_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_rabbitmq_is_available()
        {
            var connectionString = "amqp://localhost:5672";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                     .AddRabbitMQ(rabbitConnectionString: connectionString, tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_rabbitmq_is_available_using_ssloption()
        {
            var connectionString = "amqp://localhost:5672";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                     .AddRabbitMQ(rabbitConnectionString: connectionString, sslOption: new SslOption(serverName: "localhost", enabled: false), tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_rabbitmq_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddRabbitMQ("amqp://localhost:6672", sslOption: new SslOption(serverName: "localhost", enabled: false), tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_rabbitmq_is_available_using_iconnectionfactory()
        {
            var connectionString = "amqp://localhost:5672";

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption(serverName: "localhost", enabled: false)
            };

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddHealthChecks()
                        .AddRabbitMQ(sp => factory, tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_rabbitmq_is_available_using_iconnection()
        {
            var connectionString = "amqp://localhost:5672";

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption(serverName: "localhost", enabled: false)
            };

            var connection = factory.CreateConnection();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<IConnection>(connection)
                        .AddHealthChecks()
                        .AddRabbitMQ(tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_rabbitmq_is_available_and_specify_default_ssloption()
        {
            var connectionString = "amqp://localhost:5672";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                     .AddRabbitMQ(connectionString, sslOption: new SslOption(serverName: "localhost", enabled: false), tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_not_crash_on_startup_when_rabbitmq_is_down_at_startup()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<IConnectionFactory>(sp =>
                        {
                            return new ConnectionFactory()
                            {
                                Uri = new Uri("amqp://localhost:3333"),
                                AutomaticRecoveryEnabled = true,
                                Ssl = new SslOption(serverName: "localhost", enabled: false)
                            };
                        })
                        .AddHealthChecks()
                        .AddRabbitMQ(tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response1 = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
            response1.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_rabbitmq_is_available_using_iServiceProvider()
        {
            var connectionString = "amqp://localhost:5672";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddHealthChecks()
                        .AddRabbitMQ(_ => new Uri(connectionString), tags: new string[] { "rabbitmq" });

                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
