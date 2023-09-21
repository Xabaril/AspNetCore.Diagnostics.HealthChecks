using HealthChecks.NpgSql;

namespace HealthChecks.Npgsql.Tests.DependencyInjection;

public class npgsql_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNpgSql("Server=localhost");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("npgsql");
        check.ShouldBeOfType<NpgSqlHealthCheck>();

    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNpgSql("Server=localhost", name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-npg-1");
        check.ShouldBeOfType<NpgSqlHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddNpgSql(_ =>
            {
                factoryCalled = true;
                return "Server=localhost";
            }, name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-npg-1");
        check.ShouldBeOfType<NpgSqlHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void factory_is_called_only_once()
    {
        ServiceCollection services = new();
        int factoryCalls = 0;
        services.AddHealthChecks()
            .AddNpgSql(_ =>
            {
                Interlocked.Increment(ref factoryCalls);
                return "Server=localhost";
            }, name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.Single();

        for (int i = 0; i < 10; i++)
        {
            _ = registration.Factory(serviceProvider);
        }

        factoryCalls.ShouldBe(1);
    }
}
