using Testcontainers.Consul;

namespace HealthChecks.Consul.Tests;

public class ConsulContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/consul";

    private const string Tag = "1.15.4";

    public ConsulContainer? Container { get; private set; }

    public ConsulOptions GetConnectionOptions()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return new ConsulOptions
        {
            HostName = Container.Hostname,
            Port = Container.GetMappedPublicPort(ConsulBuilder.ConsulHttpPort),
            RequireHttps = false
        };
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private async Task<ConsulContainer?> CreateContainerAsync()
    {
        var container = new ConsulBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
