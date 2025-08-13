using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Gremlin.Tests;

public class GremlinContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "tinkerpop/gremlin-server";

    private const string Tag = "3.7.4";

    private const int Port = 8182;

    public IContainer? Container { get; private set; }

    public GremlinOptions GetConnectionOptions()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        var options = new GremlinOptions
        {
            Hostname = Container.Hostname,
            Port = Container.GetMappedPublicPort(Port),
            EnableSsl = false
        };

        return options;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var waitStrategy = Wait
            .ForUnixContainer()
            .UntilPortIsAvailable(Port);

        var container = new ContainerBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithPortBinding(Port, true)
            .WithWaitStrategy(waitStrategy)
            .Build();

        await container.StartAsync();

        return container;
    }
}
