using Testcontainers.Milvus;

namespace Aspire.Milvus.Client.Tests;

public sealed class MilvusContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "milvusdb/milvus";


    public const string Tag = "v2.4.13";

    public MilvusContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetEndpoint().AbsoluteUri ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync();
        }
    }

    public static async Task<MilvusContainer> CreateContainerAsync()
    {
        var container = new MilvusBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }
}
