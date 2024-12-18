using Testcontainers.PostgreSql;

namespace HealthChecks.Npgsql.Tests;

public sealed class PostgreSQLContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "library/postgres";

    public const string Tag = "17.0";

    public PostgreSqlContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<PostgreSqlContainer> CreateContainerAsync()
    {
        var container = new PostgreSqlBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
