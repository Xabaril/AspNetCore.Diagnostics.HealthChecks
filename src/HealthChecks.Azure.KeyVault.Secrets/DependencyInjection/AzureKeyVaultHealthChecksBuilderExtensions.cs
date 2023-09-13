using Azure.Security.KeyVault.Secrets;
using HealthChecks.Azure.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureKeyVaultSecretsHealthCheck"/>.
/// </summary>
public static class AzureKeyVaultHealthChecksBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "azure_key_vault_secret";

    /// <summary>
    /// Add a health check for Azure Key Vault Secrets by registering <see cref="AzureKeyVaultSecretsHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param healthCheckName="secretClientFactory">
    /// An optional factory to obtain <see cref="SecretClient" /> instance.
    /// When not provided, <see cref="SecretClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param healthCheckName="keyVaultServiceUriFactory">
    /// An optional factory to obtain <see cref="AzureKeyVaultSecretOptions"/> used by the health check.
    /// When not provided, defaults are used.</param>
    /// <param healthCheckName="healthCheckName">The health check healthCheckName. Optional. If <c>null</c> the type healthCheckName 'azure_key_vault_secret' will be used.</param>
    /// <param healthCheckName="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param healthCheckName="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param healthCheckName="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref healthCheckName="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureKeyVaultSecrets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, SecretClient>? secretClientFactory = default,
        Func<IServiceProvider, AzureKeyVaultSecretOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           string.IsNullOrEmpty(healthCheckName) ? HEALTH_CHECK_NAME : healthCheckName!,
           sp => new AzureKeyVaultSecretsHealthCheck(
                    secretClient: secretClientFactory?.Invoke(sp) ?? sp.GetRequiredService<SecretClient>(),
                    options: optionsFactory?.Invoke(sp) ?? new AzureKeyVaultSecretOptions()),
           failureStatus,
           tags,
           timeout));
    }
}
