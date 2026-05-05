using Testcontainers.MsSql;

namespace HealthChecks.UI.Tests.Fixtures;

public class SqlServerContainerFixture : IAsyncLifetime
{
    private const string Registry = "mcr.microsoft.com";

    private const string Image = "mssql/server";

    private const string Tag = "2022-CU20-GDR1-ubuntu-22.04";

    public MsSqlContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.GetConnectionString();
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<MsSqlContainer> CreateContainerAsync()
    {
        var container = new MsSqlBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
