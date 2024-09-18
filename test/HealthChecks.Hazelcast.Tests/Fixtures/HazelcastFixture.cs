using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Hazelcast.Tests.Fixtures;

public class HazelcastFixture : IAsyncLifetime
{
    public IContainer HazelcastContainer { get; private set; }

    public HazelcastFixture()
    {
        HazelcastContainer = new ContainerBuilder()
            .WithImage("hazelcast/hazelcast")
            .WithPortBinding(5701, 5701)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5701))
            .Build();
    }

    public async Task InitializeAsync() => await HazelcastContainer.StartAsync();
    public async Task DisposeAsync() => await HazelcastContainer.StopAsync();
}

