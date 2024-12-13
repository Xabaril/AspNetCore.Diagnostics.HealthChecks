using StatsdClient;

namespace HealthChecks.Publisher.Datadog.Tests.DependencyInjection;

public class datadog_publisher_registration_should
{
    [Fact]
    public void add_healthcheck_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddSingleton(sp =>
            {
                StatsdConfig config = new() { StatsdServerName = "127.0.0.1" };
                DogStatsdService service = new();
                service.Configure(config);
                return service;
            })
            .AddHealthChecks()
            .AddDatadogPublisher(serviceCheckName: "serviceCheckName")
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

        publisher.ShouldNotBeNull();
    }

    [Fact]
    public void CodeFromReadmeCompiles()
    {
        Defaults(new ServiceCollection().AddHealthChecks());
        Customization(new ServiceCollection().AddHealthChecks());

        void Defaults(IHealthChecksBuilder builder)
        {
            builder.Services.AddSingleton(sp =>
            {
                StatsdConfig config = new() { StatsdServerName = "127.0.0.1" };
                DogStatsdService service = new();
                service.Configure(config);
                return service;
            });
            builder.AddDatadogPublisher(serviceCheckName: "myservice.healthchecks");
        }

        void Customization(IHealthChecksBuilder builder)
        {
            builder.AddDatadogPublisher(
                serviceCheckName: "myservice.healthchecks",
                sp => new StatsdConfig()
                {
                    StatsdServerName = "127.0.0.1",
                    StatsdPort = 123,
                });
        }
    }
}
