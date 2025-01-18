using Neo4jClient;

namespace HealthChecks.Neo4jClient.Tests.DependencyInjection;

public class RegistrationTests
{
    [Fact]
    public async Task add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var boltClient = new BoltGraphClient("bolt://localhost:7687", "neo4j", "P@ssword");
        await boltClient.ConnectAsync();
        await boltClient.Cypher
            .Create("(a:Test{Name: $param})")
            .WithParam("param", "name123")
            .ExecuteWithoutResultsAsync();

        services.AddSingleton(boltClient);

        services.AddHealthChecks()
            .AddNeo4jClient(f => f.GetRequiredService<IGraphClient>());

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        registration.Name.ShouldBe("neo4j");
    }

    [Fact]
    public async Task add_health_check_when_an_instance_of_bolt_graph_client_is_passed_to_options_class()
    {
        var services = new ServiceCollection();
        var boltClient = new BoltGraphClient("bolt://localhost:7687", "neo4j", "P@ssword");
        await boltClient.ConnectAsync();
        await boltClient.Cypher
            .Create("(a:Test{Name: $param})")
            .WithParam("param", "name123")
            .ExecuteWithoutResultsAsync();

        services.AddSingleton(boltClient);

        var healthCheckOptions = new Neo4jClientHealthCheckOptions(boltClient);

        services.AddHealthChecks()
            .AddNeo4jClient(healthCheckOptions);

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        registration.Name.ShouldBe("neo4j");
    }

    [Fact]
    public async Task add_health_check_when_bolt_graph_client_configured_from_options_class()
    {
        var services = new ServiceCollection();
        var healthCheckOptions = new Neo4jClientHealthCheckOptions("bolt://localhost:7687", "neo4j", "P@ssword", null);

        services.AddHealthChecks()
            .AddNeo4jClient(healthCheckOptions);

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        registration.Name.ShouldBe("neo4j");
    }
}
