using Testcontainers.ArangoDb;

namespace HealthChecks.ArangoDb.Tests;

public class ArangoDbContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/arangodb";

    private const string Tag = "3.12.5.2";

    public ArangoDbContainer? Container { get; private set; }

    public ArangoDbOptions GetConnectionOptions()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        var options = new ArangoDbOptions
        {
            HostUri = Container.GetTransportAddress(),
            UserName = ArangoDbBuilder.DefaultUsername,
            Password = ArangoDbBuilder.DefaultPassword,
        };

        return options;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private async Task<ArangoDbContainer?> CreateContainerAsync()
    {
        var container = new ArangoDbBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
