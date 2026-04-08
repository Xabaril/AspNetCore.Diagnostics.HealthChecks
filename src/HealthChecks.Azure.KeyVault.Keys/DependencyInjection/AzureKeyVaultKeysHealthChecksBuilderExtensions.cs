using Azure.Security.KeyVault.Keys;
using HealthChecks.Azure.KeyVault.Keys;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class AzureKeyVaultKeysHealthChecksBuilderExtensions
{
    private const string NAME = "azure_key_vault_key";

    public static IHealthChecksBuilder AddAzureKeyVaultKeys(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, KeyClient>? clientFactory = default,
        Func<IServiceProvider, AzureKeyVaultKeysHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new AzureKeyVaultKeysHealthCheck(
                keyClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<KeyClient>(),
                options: optionsFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
