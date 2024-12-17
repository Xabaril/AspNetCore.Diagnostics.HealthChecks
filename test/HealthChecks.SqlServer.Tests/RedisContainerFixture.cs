using Testcontainers.MsSql;

namespace HealthChecks.SqlServer.Tests;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    public const string Registry = "mcr.microsoft.com";

    public const string Image = "mssql/server";

    public const string Tag = "2022-latest";

    public MsSqlContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<MsSqlContainer> CreateContainerAsync()
    {
        var container = new MsSqlBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
