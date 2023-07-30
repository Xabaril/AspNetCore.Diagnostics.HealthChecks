namespace HealthChecks.Publisher.Datadog.Tests.DependencyInjection;

public class seq_publisher_registration_should
{
    [Fact]
    public void add_healthcheck_when_properly_configured()
    {
        var services = new ServiceCollection();
        services
            .AddHealthChecks()
            .AddSeqPublisher(setup =>
            {
                setup.Endpoint = "endpoint";
                setup.DefaultInputLevel = Seq.SeqInputLevel.Information;
                setup.ApiKey = "apiKey";
            });

        using var serviceProvider = services.BuildServiceProvider();
        var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

        publisher.ShouldNotBeNull();
    }
}
