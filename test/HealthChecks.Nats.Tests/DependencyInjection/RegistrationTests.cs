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
        check.ShouldBeOfType<NatsHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNats(clientFactory: ClientFactory, name: "custom-nats");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("custom-nats");
        check.ShouldBeOfType<NatsHealthCheck>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void add_health_check_when_properly_configured_by_using_singlton_regestration(bool registerAsAbstraction)
    {
        var services = new ServiceCollection();
        var natsOpts = NatsOpts.Default with
        {
            Url = DefaultLocalConnectionString,
        };
        var connection = new NatsConnection(natsOpts);

        if (registerAsAbstraction)
        {
            services.AddSingleton<INatsConnection>(connection);
        }
        else
        {
            services.AddSingleton(connection);
        }

        services.AddHealthChecks()
              .AddNats();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("nats");
        check.ShouldBeOfType<NatsHealthCheck>();

        if (registerAsAbstraction)
        {
            serviceProvider.GetRequiredService<INatsConnection>();
        }
        else
        {
            serviceProvider.GetRequiredService<NatsConnection>();
        }
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
