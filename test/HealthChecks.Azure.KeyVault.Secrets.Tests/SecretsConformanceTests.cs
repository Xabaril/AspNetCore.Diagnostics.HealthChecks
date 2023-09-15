using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HealthChecks.Azure.KeyVault.Secrets;

namespace HealthChecks.AzureKeyVault.Tests;

public class SecretsConformanceTests : ConformanceTests<SecretClient, AzureKeyVaultSecretsHealthCheck, AzureKeyVaultSecretsHealthCheckOptions>
{
    protected override SecretClient CreateClientForNonExistingEndpoint()
    {
        SecretClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureKeyVaultSecretsHealthCheckOptions CreateHealthCheckOptions() => new();

    protected override AzureKeyVaultSecretsHealthCheck CreateHealthCheck(SecretClient client, AzureKeyVaultSecretsHealthCheckOptions? options)
        => new(client, options);

    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, SecretClient>? clientFactory = null, Func<IServiceProvider, AzureKeyVaultSecretsHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureKeyVaultSecrets(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
}
