using Testcontainers.MongoDb;

namespace HealthChecks.MongoDb.Tests;

public sealed class MongoDbContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "library/mongo";

    public const string Tag = "8.0";

    public MongoDbContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<MongoDbContainer> CreateContainerAsync()
    {
        var container = new MongoDbBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithUsername(null)
            .WithPassword(null)
            .Build();
        await container.StartAsync();

        return container;
    }
}
