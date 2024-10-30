using Qdrant.Client;

namespace HealthChecks.Qdrant.Tests.DependencyInjection;

public class qdrant_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddQdrant(_clientFactory);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("qdrant");
        check.ShouldBeOfType<QdrantHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddQdrant(clientFactory: _clientFactory, name: "qdrant");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("qdrant");
        check.ShouldBeOfType<QdrantHealthCheck>();
    }

    [Fact]
    public void client_should_resolve_from_di()
    {
        var client = new QdrantClient("localhost");
        var services = new ServiceCollection();
        services.AddSingleton(client);

        services.AddHealthChecks()
            .AddQdrant();

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("qdrant");
        check.ShouldBeOfType<QdrantHealthCheck>();
    }

    private QdrantClient _clientFactory(IServiceProvider _)
        => new("localhost");
}
