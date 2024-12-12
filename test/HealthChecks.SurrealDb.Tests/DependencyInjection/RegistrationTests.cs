using SurrealDb.Net;

namespace HealthChecks.SurrealDb.Tests.DependencyInjection;

public class surrealdb_registration_should
{
    private const string ConnectionString = "Server=http://localhost:8000;Namespace=test;Database=test;Username=root;Password=root";

    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSurreal(ConnectionString);
        services
            .AddHealthChecks()
            .AddSurreal();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("surrealdb");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSurreal(ConnectionString);
        services
            .AddHealthChecks()
            .AddSurreal(name: "my-surrealdb-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-surrealdb-1");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSurreal(ConnectionString);
        bool factoryCalled = false;
        services.AddHealthChecks()
            .AddSurreal(sp =>
            {
                factoryCalled = true;
                return sp.GetRequiredService<SurrealDbClient>();
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("surrealdb");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }
}
