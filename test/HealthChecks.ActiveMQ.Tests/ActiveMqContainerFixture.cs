using Testcontainers.ActiveMq;

namespace HealthCheck.Activemq.Tests;

public sealed class ArtemisContainerTest : IAsyncLifetime
{
    public const string Registry = "docker.io";
    public const string Image = "apache/activemq-artemis";
    public const string Tag = "latest";
    public const string Username = "artemis";
    public const string Password = "artemis";

    public ArtemisContainer? Container { get; private set; }

    public string GetHost() => Container?.Hostname ?? throw new InvalidOperationException("The test container was not initialized.");

    public int GetPort() => Container?.GetMappedPublicPort(61616) ?? throw new InvalidOperationException("The test container was not initialized.");

    public string GetUsername() => Username;

    public string GetPassword() => Password;

    public async Task InitializeAsync()
    {
        Container = new ArtemisBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithUsername(Username)
            .WithPassword(Password)
            .Build();

        await Container.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }
}
