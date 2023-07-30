using System.Collections;
using System.Net;
using IBM.WMQ;

namespace HealthChecks.Ibmq.Tests.Functional;

public class ibmmq_healthcheck_should
{

    // Define the name of the queue manager to use (applies to all connections)
    private const string qManager = "QM1";

    // Define the name of your host connection (applies to client connections only)
    private const string hostName = "localhost(1414)";
    private const string wrongHostName = "localhost(1417)";

    // Define the name of the channel to use (applies to client connections only)
    private const string channel = "DEV.APP.SVRCONN";

    // Define the user name.
    private const string user = "app";

    // Define the password.
    private const string password = "12345678";

    [Fact]
    public async Task be_healthy_if_ibmmq_is_available()
    {
        var properties = new Hashtable
        {
            { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
            { MQC.CHANNEL_PROPERTY, channel },
            { MQC.CONNECTION_NAME_PROPERTY, hostName },
            { MQC.USER_ID_PROPERTY, user },
            { MQC.PASSWORD_PROPERTY, password }
        };

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddIbmMQ(qManager, properties, tags: new string[] { "ibmmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_ibmmq_is_unavailable()
    {
        var properties = new Hashtable
        {
            { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
            { MQC.CHANNEL_PROPERTY, channel },
            { MQC.CONNECTION_NAME_PROPERTY, wrongHostName },
            { MQC.USER_ID_PROPERTY, user },
            { MQC.PASSWORD_PROPERTY, password }
        };

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddIbmMQ(qManager, properties, tags: new string[] { "ibmmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_ibmmq_managed_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddIbmMQManagedConnection(qManager, channel, wrongHostName, user, password, tags: new string[] { "ibmmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_ibmmq_managed_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddIbmMQManagedConnection(qManager, channel, hostName, user, password, tags: new string[] { "ibmmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmmq")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
