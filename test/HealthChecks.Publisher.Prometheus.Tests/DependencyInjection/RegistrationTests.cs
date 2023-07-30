namespace HealthChecks.Publisher.ApplicationInsights.Tests.DependencyInjection;

public class prometheus_publisher_registration_should
{
    [Fact]
    [Obsolete("AddPrometheusGatewayPublisher is obsolete")]
    public void add_healthcheck_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddPrometheusGatewayPublisher("http://endpoint.com", "job_name")
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

        publisher.ShouldNotBeNull();
    }
}
