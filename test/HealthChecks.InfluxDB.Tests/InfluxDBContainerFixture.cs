using Testcontainers.InfluxDb;

namespace HealthChecks.InfluxDB.Tests;

public class InfluxDBContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/influxdb";

    private const string Tag = "2.7.12-alpine";

    public InfluxDbContainer? Container { get; private set; }

    public (string Address, string Username, string Password) GetConnectionProperties()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return (Container.GetAddress(), InfluxDbBuilder.DefaultUsername, InfluxDbBuilder.DefaultPassword);
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<InfluxDbContainer> CreateContainerAsync()
    {
        var container = new InfluxDbBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
