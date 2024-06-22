using VaultSharp;

namespace HealthCheks.Vault;

internal class VaultHealthCheckOptions<TAuthMethodInfo> where TAuthMethodInfo : IVaultAuthMethodInfo
{
    // Vault address, e.g., "http://127.0.0.1:8200"
    public string? VaultAddress { get; set; }

    // Authentication method information
    public TAuthMethodInfo? AuthMethodInfo { get; set; }

    // Optional: Additional configuration for the Vault client
    public Action<VaultClientSettings>? ConfigureVaultClient { get; set; }
}
