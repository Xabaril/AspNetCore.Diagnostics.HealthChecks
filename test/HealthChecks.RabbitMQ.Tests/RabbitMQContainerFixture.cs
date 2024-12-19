using Testcontainers.RabbitMq;

namespace HealthChecks.RabbitMQ.Tests;

public sealed class RabbitMQContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "library/rabbitmq";

    public const string Tag = "4.0";

    public RabbitMqContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<RabbitMqContainer> CreateContainerAsync()
    {
        var container = new RabbitMqBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
