using System.Collections;
using System.Net;
using IBM.WMQ;

namespace HealthChecks.IbmMQ.Tests.Functional;

public class ibmmq_healthcheck_should(IbmMQContainerFixture ibmMqFixture) : IClassFixture<IbmMQContainerFixture>
{
    private const string wrongHostName = "localhost(1417)";

    [Fact]
    public async Task be_healthy_if_ibmmq_is_available()
    {
        var properties = ibmMqFixture.GetConnectionProperties();

        var connectionProperties = new Hashtable
        {
            { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
            { MQC.CHANNEL_PROPERTY, properties.Channel },
            { MQC.CONNECTION_NAME_PROPERTY, properties.Hostname },
            { MQC.USER_ID_PROPERTY, properties.Username },
            { MQC.PASSWORD_PROPERTY, properties.Password }
        };

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddIbmMQ(properties.QueueManager, connectionProperties, tags: ["ibmmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_ibmmq_is_unavailable()
    {
        var properties = ibmMqFixture.GetConnectionProperties();

        var connectionProperties = new Hashtable
        {
            { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
            { MQC.CHANNEL_PROPERTY, properties.Channel },
            { MQC.CONNECTION_NAME_PROPERTY, wrongHostName },
            { MQC.USER_ID_PROPERTY, properties.Username },
            { MQC.PASSWORD_PROPERTY, properties.Password }
        };

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddIbmMQ(properties.QueueManager, connectionProperties, tags: ["ibmmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_ibmmq_managed_is_unavailable()
    {
        var properties = ibmMqFixture.GetConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddIbmMQManagedConnection(
                        properties.QueueManager,
                        properties.Channel,
                        wrongHostName,
                        properties.Username,
                        properties.Password,
                        tags: ["ibmmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_ibmmq_managed_is_available()
    {
        var properties = ibmMqFixture.GetConnectionProperties();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddIbmMQManagedConnection(
                        properties.QueueManager,
                        properties.Channel,
                        properties.Hostname,
                        properties.Username,
                        properties.Password,
                        tags: ["ibmmq"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
