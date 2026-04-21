using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;

namespace HealthChecks.Oracle.Tests;

public class OracleContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "gvenzl/oracle-xe";

    private const string Tag = "21.3.0-slim-faststart";

    public OracleContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.GetConnectionString();
    }

    public OracleConnectionStringBuilder GetConnectionStringBuilder() => new(GetConnectionString());

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private async Task<OracleContainer> CreateContainerAsync()
    {
        var container = new OracleBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
