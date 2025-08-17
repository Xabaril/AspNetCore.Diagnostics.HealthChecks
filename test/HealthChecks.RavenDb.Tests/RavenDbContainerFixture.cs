using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Testcontainers.RavenDb;

namespace HealthChecks.RavenDb.Tests;

public class RavenDbContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "ravendb/ravendb";

    private const string Tag = "7.1-latest";

    public RavenDbContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.GetConnectionString();
    }

    public async Task InitializeAsync()
    {
        Container = await CreateContainerAsync();

        using var store = new DocumentStore();

        store.Urls = [GetConnectionString()];

        store.Initialize();

        await store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord("Demo")));
    }

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private async Task<RavenDbContainer?> CreateContainerAsync()
    {
        var container = new RavenDbBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
