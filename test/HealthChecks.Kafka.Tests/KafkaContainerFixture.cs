using Testcontainers.Kafka;

namespace HealthChecks.Kafka.Tests;

public sealed class KafkaContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "confluentinc/cp-kafka";

    public const string Tag = "7.8.0";

    public KafkaContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetBootstrapAddress() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<KafkaContainer> CreateContainerAsync()
    {
        var container = new KafkaBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
