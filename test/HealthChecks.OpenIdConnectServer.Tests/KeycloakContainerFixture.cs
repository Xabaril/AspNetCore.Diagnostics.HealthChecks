using Testcontainers.Keycloak;

namespace HealthChecks.OpenIdConnectServer.Tests;

public class KeycloakContainerFixture : IAsyncLifetime
{
    private const string Registry = "quay.io";

    private const string Image = "keycloak/keycloak";

    private const string Tag = "26.3.2";

    public KeycloakContainer? Container { get; private set; }

    public string GetBaseAddress()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        var uriBuilder = new UriBuilder(Container.GetBaseAddress())
        {
            Path = "/realms/master/"
        };

        return uriBuilder.ToString();
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<KeycloakContainer> CreateContainerAsync()
    {
        var container = new KeycloakBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
