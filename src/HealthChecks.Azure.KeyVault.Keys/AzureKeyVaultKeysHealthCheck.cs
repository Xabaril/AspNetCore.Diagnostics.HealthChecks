using Azure;
using Azure.Security.KeyVault.Keys;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.KeyVault.Keys;

public sealed class AzureKeyVaultKeysHealthCheck : IHealthCheck
{
    private readonly KeyClient _keyClient;
    private readonly AzureKeyVaultKeysHealthCheckOptions _options;

    public AzureKeyVaultKeysHealthCheck(KeyClient keyClient, AzureKeyVaultKeysHealthCheckOptions? options = default)
    {
        _keyClient = Guard.ThrowIfNull(keyClient);
        _options = options ?? new AzureKeyVaultKeysHealthCheckOptions();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string keyName = _options.KeyName;

        try
        {
            await _keyClient.GetKeyAsync(keyName, cancellationToken: cancellationToken).ConfigureAwait(false);
            return new HealthCheckResult(HealthStatus.Healthy);
        }
        catch (RequestFailedException azureEx) when (azureEx.Status == 404)
        {
            return new HealthCheckResult(HealthStatus.Healthy);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
