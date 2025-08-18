namespace HealthChecks.Publisher.OpenTelemetry.Tests.DependencyInjection;

public class open_telemetry_publisher_registration_should
{
    [Fact]
    public void add_healthcheck_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddOpenTelemetryPublisher()
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

        publisher.ShouldNotBeNull();
    }
}
