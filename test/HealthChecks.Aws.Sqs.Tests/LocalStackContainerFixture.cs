using Testcontainers.LocalStack;

namespace HealthChecks.Aws.Sqs.Tests;

public class LocalStackContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "localstack/localstack";

    private const string Tag = "4.7.0";

    public LocalStackContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.GetConnectionString();
    }

    public async Task InitializeAsync()
    {
        Container = await CreateContainerAsync();

        await Container.ExecAsync(["awslocal", "sqs", "create-queue", "--queue-name", "healthchecks"]);
    }

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<LocalStackContainer> CreateContainerAsync()
    {
        var container = new LocalStackBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
