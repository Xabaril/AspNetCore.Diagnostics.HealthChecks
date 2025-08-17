using System.Data.Common;
using Testcontainers.EventStoreDb;

namespace HealthChecks.EventStore.Tests;

public class EventStoreDbContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "eventstore/eventstore";

    private const string Tag = "22.10.5-bookworm-slim";

    public const int TcpPort = 1113;

    public EventStoreDbContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        var uriBuilder = new UriBuilder(
            "tcp",
            Container.Hostname,
            Container.GetMappedPublicPort(TcpPort));

        var dbConnectionStringBuilder = new DbConnectionStringBuilder
        {
            { "ConnectTo", uriBuilder.ToString() },
            { "UseSslConnection", false }
        };

        return dbConnectionStringBuilder.ConnectionString;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<EventStoreDbContainer> CreateContainerAsync()
    {
        var container = new EventStoreDbBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithEnvironment("EVENTSTORE_ENABLE_EXTERNAL_TCP", "true")
            .WithPortBinding(TcpPort, true)
            .Build();

        await container.StartAsync();

        return container;
    }
}
