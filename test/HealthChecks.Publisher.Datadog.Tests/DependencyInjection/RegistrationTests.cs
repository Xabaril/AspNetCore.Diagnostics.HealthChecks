namespace HealthChecks.Publisher.Datadog.Tests.DependencyInjection;

public class datadog_publisher_registration_should
{
    [Fact]
    public void add_healthcheck_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddDatadogPublisher(serviceCheckName: "serviceCheckName", datadogAgentName: "127.0.0.1")
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

        publisher.ShouldNotBeNull();
    }
}
