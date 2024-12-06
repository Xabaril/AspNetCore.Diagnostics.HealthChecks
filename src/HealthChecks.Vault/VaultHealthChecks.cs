using Microsoft.Extensions.Diagnostics.HealthChecks;
using VaultSharp;

namespace HealthChecks.Vault;

public class VaultHealthChecks : IHealthCheck
{
    private readonly IVaultClient _vaultClient;

    public VaultHealthChecks(IVaultClient vaultClient)
    {
        _vaultClient = Guard.ThrowIfNull(vaultClient);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await _vaultClient.V1.System
                .GetHealthStatusAsync()
                .ConfigureAwait(false);

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
