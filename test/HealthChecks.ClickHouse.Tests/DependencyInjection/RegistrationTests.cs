using ClickHouse.Client.ADO;

namespace HealthChecks.ClickHouse.Tests.DependencyInjection;

public class clickhouse_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddClickHouse(static _ => new ClickHouseConnection("Host=localhost"));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("ClickHouse");
        check.ShouldBeOfType<ClickHouseHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddClickHouse(static _ => new ClickHouseConnection("Host=localhost"), name: "my-ch-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-ch-1");
        check.ShouldBeOfType<ClickHouseHealthCheck>();
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
                return new ClickHouseConnection("Host=localhost");
            }, name: "my-ch-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-ch-1");
        check.ShouldBeOfType<ClickHouseHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void factory_is_called_everytime_healthcheck_is_created()
    {
        ServiceCollection services = new();
        int factoryCalls = 0;
        services.AddHealthChecks()
            .AddClickHouse(_ =>
            {
                Interlocked.Increment(ref factoryCalls);
                return new ClickHouseConnection("Host=localhost");
            }, name: "my-ch-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.Single();

        for (int i = 0; i < 10; i++)
        {
            _ = registration.Factory(serviceProvider);
        }

        // ClickHouseConnection is not thread safe, so we assume that that we get a new instance every time
        factoryCalls.ShouldBe(10);
    }
}
