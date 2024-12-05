using NATS.Client.Core;
using static HealthChecks.Nats.Tests.Defines;

namespace HealthChecks.Nats.Tests.DependencyInjection;

public class nats_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNats(ClientFactory);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("nats");
        check.ShouldBeOfType<NatsConnection>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNats(clientFactory: ClientFactory, name: "nats");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("nats");
        check.ShouldBeOfType<NatsConnection>();
    }

    private NatsConnection ClientFactory(IServiceProvider _)
    {
        var options = NatsOpts.Default with
        {
            Url = DefaultLocalConnectionString,
        };
        return new NatsConnection(options);
    }
}
