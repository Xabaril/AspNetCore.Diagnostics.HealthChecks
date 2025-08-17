using Testcontainers.ClickHouse;

namespace HealthChecks.ClickHouse.Tests;

public class ClickHouseContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/clickhouse";

    private const string Tag = "25.7.2";

    public ClickHouseContainer? Container { get; private set; }

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

    private async Task<ClickHouseContainer?> CreateContainerAsync()
    {
        var container = new ClickHouseBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
