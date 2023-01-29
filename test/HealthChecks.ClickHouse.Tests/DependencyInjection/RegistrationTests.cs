namespace HealthChecks.ClickHouse.Tests.DependencyInjection;

public class clickHouse_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddClickHouse("connectionstring");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("clickHouse");
        check.GetType().ShouldBe(typeof(ClickHouseHealthCheck));
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddClickHouse("connectionstring", name: "my-click-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-click-1");
        check.GetType().ShouldBe(typeof(ClickHouseHealthCheck));
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddClickHouse(_ =>
            {
                factoryCalled = true;
                return "connectionstring";
            }, name: "my-click-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-click-1");
        check.GetType().ShouldBe(typeof(ClickHouseHealthCheck));
        factoryCalled.ShouldBeTrue();
    }
}
