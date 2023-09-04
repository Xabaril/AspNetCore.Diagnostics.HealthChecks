using static HealthChecks.Nats.Tests.Defines;

namespace HealthChecks.Nats.Tests.DependencyInjection;

public class nats_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_locally() =>
        RegistrationFact(
            _ => _.Url = DefaultLocalConnectionString,
            check => check.ShouldBeOfType<NatsHealthCheck>());

    [Fact]
    public void add_named_health_check_when_properly_configured_multiple_local_instances() =>
        RegistrationFact(
            _ => _.Url = MixedLocalUrl,
            check => check.ShouldBeOfType<NatsHealthCheck>(),
            name: CustomRegistrationName);

    [Fact]
    public void add_health_check_when_demo_instance_properly_configured() =>
        RegistrationFact(
            setup => setup.Url = DemoConnectionString,
            check => check.ShouldBeOfType<NatsHealthCheck>());

    [Fact]
    public void add_health_check_with_service_provider()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IDependency, Dependency>()
            .AddHealthChecks()
            .AddNats((sp, setup) => setup.Url = sp.GetRequiredService<IDependency>().ConnectionString);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        registration.Name.ShouldBe(NatsName);

        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<NatsHealthCheck>();
    }

    private void RegistrationFact(Action<NatsOptions> setup, Action<IHealthCheck> assert, string? name = null)
    {
        var services = new ServiceCollection();
        services.AddHealthChecks().AddNats(setup, name);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        registration.Name.ShouldBe(name ?? NatsName);

        var check = registration.Factory(serviceProvider);
        assert(check);
    }

    private interface IDependency
    {
        string ConnectionString { get; }
    }

    private class Dependency : IDependency
    {
        public string ConnectionString => DemoConnectionString;
    }
}
