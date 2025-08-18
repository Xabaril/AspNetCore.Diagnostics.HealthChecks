using Testcontainers.Nats;

namespace HealthChecks.Nats.Tests;

public class NatsContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/nats";

    private const string Tag = "2.11.8";

    public NatsContainer? Container { get; private set; }

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

    private static async Task<NatsContainer> CreateContainerAsync()
    {
        var container = new NatsBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
