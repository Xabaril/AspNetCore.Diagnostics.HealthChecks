using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;

namespace HealthChecks.Vault.Tests.Fixtures;

public class VaultContainerFixture : IAsyncLifetime
{
    public string RootToken { get; } = "root";

    private readonly IContainer _container;

    public string GetConnectionString()
    {
        if (_container is null)
            throw new InvalidOperationException("container is not initialized");
        return $"http://localhost:{_container.GetMappedPublicPort(8200)}";
    }

    public VaultContainerFixture()
    {
        _container = new ContainerBuilder()
            .WithImage("hashicorp/vault:1.18")
            .WithPortBinding(8200, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8200)))
            .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", RootToken)
            .WithEnvironment("VAULT_LOCAL_CONFIG", "{\"backend\": {\"file\": {\"path\": \"/vault/file\"}}, \"default_lease_ttl\": \"168h\", \"max_lease_ttl\": \"720h\"}")
            .WithCommand($"server -dev -dev-root-token-id={RootToken}")
            .Build();
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync();
        }
    }
}
