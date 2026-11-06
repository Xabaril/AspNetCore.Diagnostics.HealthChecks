using Testcontainers.Neo4j;

namespace HealthChecks.Neo4j.Tests;

public sealed class Neo4jContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "library/neo4j";

    public const string Tag = "5.4";

    public Neo4jContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<Neo4jContainer> CreateContainerAsync()
    {
        var container = new Neo4jBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithEnvironment("NEO4J_AUTH", "neo4j/password")
            .Build();
        await container.StartAsync();

        return container;
    }
}
