using Testcontainers.MySql;

namespace HealthChecks.MySql.Tests;

public sealed class MySqlContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "library/mysql";

    public const string Tag = "9.1";

    public MySqlContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<MySqlContainer> CreateContainerAsync()
    {
        var container = new MySqlBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
