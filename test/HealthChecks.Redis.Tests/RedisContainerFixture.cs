using Testcontainers.Redis;

namespace HealthChecks.Redis.Tests;

public sealed class RedisContainerFixture : IAsyncLifetime
{
    public RedisContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<RedisContainer> CreateContainerAsync()
    {
        var container = new RedisBuilder()
            .WithImage($"{RedisContainerImageTags.Registry}/{RedisContainerImageTags.Image}:{RedisContainerImageTags.Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
