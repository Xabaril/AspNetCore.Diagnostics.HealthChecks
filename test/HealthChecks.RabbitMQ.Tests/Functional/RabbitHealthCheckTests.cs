using System.Net;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ.Tests.Functional;

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
                 .AddRabbitMQ(
                    _ => new ConnectionFactory() { Uri = new Uri(connectionString) }.CreateConnectionAsync(),
                    tags: ["rabbitmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_rabbitmq_is_not_available()
    {
        var connectionString = "amqp://localhost:6672";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddRabbitMQ(
                    _ => new ConnectionFactory() { Uri = new Uri(connectionString) }.CreateConnectionAsync(),
                    tags: ["rabbitmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
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

        var connection = await factory.CreateConnectionAsync();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .Add(new HealthCheckRegistration(
                        "rabbitmq",
                        _ => new RabbitMQHealthCheck(connection),
                        failureStatus: null,
                        tags: ["rabbitmq"]));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_rabbitmq_is_available_using_iconnection_in_serviceprovider()
    {
        var connectionString = "amqp://localhost:5672";

        var factory = new ConnectionFactory()
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            Ssl = new SslOption(serverName: "localhost", enabled: false)
        };

        var connection = await factory.CreateConnectionAsync();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<IConnection>(connection)
                    .AddHealthChecks()
                    .AddRabbitMQ(tags: ["rabbitmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

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
                    .AddRabbitMQ(
                        sp => sp.GetRequiredService<IConnectionFactory>().CreateConnectionAsync(),
                        tags: ["rabbitmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response1 = await server.CreateRequest("/health").GetAsync();
        response1.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task two_rabbitmq_health_check()
    {
        const string connectionString1 = "amqp://localhost:5672";
        const string connectionString2 = "amqp://localhost:6672/";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddKeyedSingleton("1", (sp, _) => new ConnectionFactory() { Uri = new Uri(connectionString1) }.CreateConnectionAsync().GetAwaiter().GetResult());
                services.AddKeyedSingleton("2", (sp, _) => new ConnectionFactory() { Uri = new Uri(connectionString2) }.CreateConnectionAsync().GetAwaiter().GetResult());

                services.AddHealthChecks()
                    .AddRabbitMQ(sp => sp.GetRequiredKeyedService<IConnection>("1"), name: "rabbitmq1")
                    .AddRabbitMQ(sp => sp.GetRequiredKeyedService<IConnection>("2"), name: "rabbitmq2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health1", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Equals("rabbitmq1")
                });
                app.UseHealthChecks("/health2", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Equals("rabbitmq2")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response1 = await server.CreateRequest("/health1").GetAsync();
        using var response2 = await server.CreateRequest("/health2").GetAsync();

        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task no_connection_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRabbitMQ(tags: ["rabbitmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response1 = await server.CreateRequest("/health").GetAsync();
        response1.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
