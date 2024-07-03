using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;

namespace HealthChecks.Vault.Tests.Fixtures;

public class VaultContainerFixture
{
    private readonly IContainer? _container;

    public string VaultAddress => $"http://localhost:{_container!.GetMappedPublicPort(8200)}";
    public string? RootToken { get; private set; }

    public VaultContainerFixture()
    {
        _container = new ContainerBuilder()
            .WithImage("hashicorp/vault:latest")
            .WithPortBinding(8200, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8200)))
            .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", "root")
            .WithEnvironment("VAULT_LOCAL_CONFIG", "{\"backend\": {\"file\": {\"path\": \"/vault/file\"}}, \"default_lease_ttl\": \"168h\", \"max_lease_ttl\": \"720h\"}")
            .Build();
    }

    public async Task StartContainerAsync()
    {
        await _container!.StartAsync().ConfigureAwait(false);
        RootToken = "root";
    }

    public async Task StopContainerAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync().ConfigureAwait(false);
        }
    }
}
