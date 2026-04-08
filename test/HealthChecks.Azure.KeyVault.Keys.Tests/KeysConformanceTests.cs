using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using HealthChecks.Azure.KeyVault.Keys;

namespace HealthChecks.Azure.KeyVault.Keys.Tests;

public class KeysConformanceTests : ConformanceTests<KeyClient, AzureKeyVaultKeysHealthCheck, AzureKeyVaultKeysHealthCheckOptions>
{
    protected override KeyClient CreateClientForNonExistingEndpoint()
    {
        KeyClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureKeyVaultKeysHealthCheckOptions CreateHealthCheckOptions() => new();

    protected override AzureKeyVaultKeysHealthCheck CreateHealthCheck(KeyClient client, AzureKeyVaultKeysHealthCheckOptions? options)
        => new(client, options);

    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, KeyClient>? clientFactory = null, Func<IServiceProvider, AzureKeyVaultKeysHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureKeyVaultKeys(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
}
