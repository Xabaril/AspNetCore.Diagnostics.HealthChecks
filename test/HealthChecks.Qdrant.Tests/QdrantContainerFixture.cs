using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Qdrant.Tests;

public sealed class QdrantContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "qdrant/qdrant";

    public const string Tag = "v1.12.1";

    private const int GrpcPort = 6334;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }
        string endpoint = new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(GrpcPort)).ToString();
        return endpoint;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder()
              .WithImage($"{Registry}/{Image}:{Tag}")
              .WithPortBinding(GrpcPort, true)
              .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(GrpcPort))
              .Build();
        await container.StartAsync();

        return container;
    }
}
