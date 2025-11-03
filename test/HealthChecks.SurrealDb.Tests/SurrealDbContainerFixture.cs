using System.Data.Common;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.SurrealDb.Tests;

public class SurrealDbContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "surrealdb/surrealdb";

    private const string Tag = "v2.3.7";

    private const int Port = 8000;

    private const string Username = "root";

    private const string Password = "root";

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        string endpoint = new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(Port)).ToString();

        var builder = new DbConnectionStringBuilder
        {
            { "Server", endpoint },
            { "Username", Username },
            { "Password", Password }
        };

        return builder.ConnectionString;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var waitStrategy = Wait
            .ForUnixContainer()
            .UntilHttpRequestIsSucceeded(x => x.ForPath("/health").ForPort(Port));

        var container = new ContainerBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithPortBinding(Port, true)
            .WithCommand("start", "--user", Username, "--pass", Password, "memory")
            .WithWaitStrategy(waitStrategy)
            .Build();

        await container.StartAsync();

        return container;
    }
}
