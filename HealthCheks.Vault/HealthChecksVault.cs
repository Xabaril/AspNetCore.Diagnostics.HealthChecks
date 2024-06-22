using Microsoft.Extensions.Diagnostics.HealthChecks;
using VaultSharp;

namespace HealthCheks.Vault;


internal class HealthChecksVault<TAuthMethodInfo> : IHealthCheck where TAuthMethodInfo : IVaultAuthMethodInfo
{
    private readonly VaultHealthCheckOptions<TAuthMethodInfo> _options;
    private IVaultClient? _vaultClient;

    public HealthChecksVault(VaultHealthCheckOptions<TAuthMethodInfo> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        InitializeVaultClient();
    }

    private void InitializeVaultClient()
    {
        var authMethodInfo = _options.AuthMethodInfo?.GetAuthMethodInfo();
        var vaultClientSettings = new VaultClientSettings(_options.VaultAddress, authMethodInfo);

        _options.ConfigureVaultClient?.Invoke(vaultClientSettings);

        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await _vaultClient!.V1.System.GetHealthStatusAsync().ConfigureAwait(false);

            if (healthStatus.Initialized && healthStatus.Sealed)
                return new HealthCheckResult(context.Registration.FailureStatus, description: "Vault is initialized but sealed.");
            else if (!healthStatus.Initialized)
                return new HealthCheckResult(context.Registration.FailureStatus, description: "Vault is not initialized.");

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}

