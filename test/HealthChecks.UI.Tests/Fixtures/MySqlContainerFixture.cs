using Testcontainers.MySql;

namespace HealthChecks.UI.Tests.Fixtures;

public class MySqlContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/mysql";

    private const string Tag = "9.4.0";

    public MySqlContainer? Container { get; private set; }

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

    private static async Task<MySqlContainer> CreateContainerAsync()
    {
        var container = new MySqlBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
