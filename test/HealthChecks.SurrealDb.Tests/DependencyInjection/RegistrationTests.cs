using SurrealDb.Net;

namespace HealthChecks.SurrealDb.Tests.DependencyInjection;

public class surrealdb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddSurreal("connectionstring")
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("surrealdb");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
    }

    [Fact]
    public async Task invoke_beforeOpen_when_defined()
    {
        var services = new ServiceCollection();
        bool invoked = false;
        const string connectionstring = "Server=http://localhost:8000;Namespace=test;Database=test;Username=root;Password=root";
        void beforeOpen(ISurrealDbClient client)
        {
            invoked = true;
            client.Uri.AbsoluteUri.ShouldBe("http://localhost:8000");
        }
        services.AddHealthChecks()
            .AddSurreal(connectionstring, configure: beforeOpen);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        await Record.ExceptionAsync(() => check.CheckHealthAsync(new HealthCheckContext()));
        invoked.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddSurreal("connectionstring", name: "my-surrealdb-1")
            .Services;

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
        bool factoryCalled = false;
        services.AddHealthChecks()
            .AddSurreal(_ =>
            {
                factoryCalled = true;
                return "connectionstring";
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
