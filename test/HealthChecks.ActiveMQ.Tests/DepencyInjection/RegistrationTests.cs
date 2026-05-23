using Amqp;
using Amqp.Framing;
using HealthChecks.Activemq;

namespace HealthCheck.Activemq.Tests;

public class active_registration_should
{

    private const string DEFAULT_CHECK_NAME = "activemq";

    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConnection>(new MockConnection());
        services.AddHealthChecks()
            .AddActiveMQ("activemq");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(DEFAULT_CHECK_NAME);
        check.ShouldBeOfType<ActiveMqHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + DEFAULT_CHECK_NAME;

        services.AddSingleton<IConnection>(new MockConnection());

        services.AddHealthChecks()
            .AddActiveMQ(name: customCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<ActiveMqHealthCheck>();
    }

    internal class MockConnection : IConnection
    {
        public Error Error => throw new NotImplementedException();
        public bool IsClosed => throw new NotImplementedException();

        public event ClosedCallback? Closed
        {
            add { }
            remove { }
        }

        public void AddClosedCallback(ClosedCallback callback) => throw new NotImplementedException();
        public void Close() => throw new NotImplementedException();
        public void Close(TimeSpan waitUntilEnded, Error error) => throw new NotImplementedException();
        public Task CloseAsync() => throw new NotImplementedException();
        public Task CloseAsync(TimeSpan timeout, Error error) => throw new NotImplementedException();
        public ISession CreateSession() => throw new NotImplementedException();
    }

}
