using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.IbmMQ.Tests;

public class IbmMQContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "ibmcom/mq";

    public const string Tag = "9.2.4.0-r1";

    private const int Port = 1414;

    private const string QueueManager = "QM1";

    private const string Channel = "DEV.APP.SVRCONN";

    private const string Username = "app";

    private const string Password = "password";

    public IContainer? Container { get; private set; }

    public (string Hostname, string Username, string Password, string Channel, string QueueManager) GetConnectionProperties()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return ($"localhost({Container.GetMappedPublicPort(Port)})", Username, Password, Channel, QueueManager);
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithEnvironment("LICENSE", "accept")
            .WithEnvironment("MQ_QMGR_NAME", QueueManager)
            .WithEnvironment("MQ_APP_PASSWORD", Password)
            .WithPortBinding(Port, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("chkmqready"))
            .Build();

        await container.StartAsync();

        return container;
    }
}
