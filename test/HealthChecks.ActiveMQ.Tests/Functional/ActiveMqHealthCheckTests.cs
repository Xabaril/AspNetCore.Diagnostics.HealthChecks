using System.Net;
using Amqp;
using HealthChecks.Activemq;


namespace HealthCheck.Activemq.Tests;

public class activemq_healthcheck_should : IClassFixture<ArtemisContainerTest>
{
    private readonly ArtemisContainerTest _activeMQContainerFixture;

    public activemq_healthcheck_should(ArtemisContainerTest activeMQContainerFixture)
    {
        _activeMQContainerFixture = activeMQContainerFixture;
    }

    [Fact]
    public async Task be_unhealthy_if_active_is_not_available()
    {

        var connectAddress = new Amqp.Address(_activeMQContainerFixture.GetHost(),
            _activeMQContainerFixture.GetPort(),
            _activeMQContainerFixture.GetUsername(),
            _activeMQContainerFixture.GetPassword(),
            "/",
            "amqp");

        var connectionFactory = new ConnectionFactory();
        var connection = await connectionFactory.CreateAsync(connectAddress);


        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConnection>(connection);

                services.AddHealthChecks()
                 .AddActiveMQ(
                    "activemq",
                    tags: ["activemq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("activemq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        await connection.CloseAsync();
        // Simulate the ActiveMQ server being unavailable
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_active_is_available_using_iconnection()
    {
        var connectAddress = new Amqp.Address(_activeMQContainerFixture.GetHost(),
            _activeMQContainerFixture.GetPort(),
            _activeMQContainerFixture.GetUsername(),
            _activeMQContainerFixture.GetPassword(),
            "/",
            "amqp");

        var connectionFactory = new ConnectionFactory();
        var connection = await connectionFactory.CreateAsync(connectAddress);

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .Add(new HealthCheckRegistration(
                        "activemq",
                        _ => new ActiveMqHealthCheck(connection),
                        failureStatus: null,
                        tags: ["activemq"]));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("activemq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_active_is_available_using_iconnection_passing()
    {
        // Using the ArtemisContainerTest fixture to get the ActiveMQ connection details
        var connectAddress = new Amqp.Address(_activeMQContainerFixture.GetHost(),
            _activeMQContainerFixture.GetPort(),
            _activeMQContainerFixture.GetUsername(),
            _activeMQContainerFixture.GetPassword(),
            "/",
            "amqp");

        var connectionFactory = new ConnectionFactory();
        var connection = await connectionFactory.CreateAsync(connectAddress);

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddActiveMQ(name: "activemq", connection: connection, tags: ["activemq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("activemq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

}
