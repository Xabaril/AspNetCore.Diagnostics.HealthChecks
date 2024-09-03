using Milvus.Client;

namespace HealthChecks.Milvus.Tests.DependencyInjection;

public class milvus_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMilvus(_clientFactory);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("milvus");
        check.ShouldBeOfType<MilvusHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMilvus(clientFactory: _clientFactory, name: "milvus");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("milvus");
        check.ShouldBeOfType<MilvusHealthCheck>();
    }

    private MilvusClient _clientFactory(IServiceProvider _)
        => new MilvusClient("host", "user", "password");
}
