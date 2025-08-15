using Testcontainers.EventStoreDb;

namespace HealthChecks.EventStore.gRPC.Tests;

public class EventStoreDbContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "eventstore/eventstore";

    private const string Tag = "24.10.6";

    public EventStoreDbContainer? Container { get; private set; }

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

    private static async Task<EventStoreDbContainer> CreateContainerAsync()
    {
        var container = new EventStoreDbBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
