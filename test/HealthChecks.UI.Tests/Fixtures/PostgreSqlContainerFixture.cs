using Testcontainers.PostgreSql;

namespace HealthChecks.UI.Tests.Fixtures;

public class PostgreSqlContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/postgres";

    private const string Tag = "17.6-alpine3.22";

    public PostgreSqlContainer? Container { get; private set; }

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

    private static async Task<PostgreSqlContainer> CreateContainerAsync()
    {
        var container = new PostgreSqlBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
