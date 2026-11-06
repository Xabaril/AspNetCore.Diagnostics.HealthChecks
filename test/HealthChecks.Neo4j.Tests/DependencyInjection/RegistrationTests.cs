using HealthChecks.Neo4j.DependencyInjection;
using Neo4j.Driver;

namespace HealthChecks.Neo4j.Tests.DependencyInjection;

public class neo4j_registration_should
{
    private const string DEFAULT_CHECK_NAME = "neo4j";
    private const string CUSTOM_CHECK_NAME = "custom-" + DEFAULT_CHECK_NAME;

    [Fact]
    public void add_health_check_when_properly_configured_neo4jClientFactory_defaults()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IDriver>(_driverFactory)
            .AddHealthChecks()
            .AddNeo4j();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(DEFAULT_CHECK_NAME);
        check.ShouldBeOfType<Neo4jHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_neo4jClientFactory_custom()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<IDriver>(_driverFactory)
            .AddHealthChecks()
            .AddNeo4j(name: CUSTOM_CHECK_NAME);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(CUSTOM_CHECK_NAME);
        check.ShouldBeOfType<Neo4jHealthCheck>();
    }

    private IDriver _driverFactory(IServiceProvider _)
        => GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic(DEFAULT_CHECK_NAME, "password"));
}
