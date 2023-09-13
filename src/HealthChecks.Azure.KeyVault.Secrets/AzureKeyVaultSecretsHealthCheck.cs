using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.KeyVault.Secrets;

/// <summary>
/// Azure Key Vault Secrets health check.
/// </summary>
public sealed class AzureKeyVaultSecretsHealthCheck : IHealthCheck
{
    private readonly SecretClient _secretClient;
    private readonly AzureKeyVaultSecretOptions _options;

    /// <summary>
    /// Creates new instance of Azure Key Vault Secrets health check.
    /// </summary>
    /// <param name="secretClient">
    /// The <see cref="SecretClient"/> used to perform Azure Key Vault operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="options">The settings used by the health check.</param>
    /// <remarks>
    /// It uses the provided <paramref name="secretClient"/> to get given <see cref="AzureKeyVaultSecretOptions.SecretName"/> secret via
    /// <see cref="SecretClient.GetSecretAsync(string, string, CancellationToken)"/> method.
    /// When the secret is not found, it will try to create it if <see cref="AzureKeyVaultSecretOptions.CreateWhenNotFound"/> is set to true.
    /// When the secret is not found, but <see cref="AzureKeyVaultSecretOptions.CreateWhenNotFound"/> is false, it returns <see cref="HealthStatus.Healthy"/> status,
    /// as the connection to the service itself can be made.
    /// </remarks>
    public AzureKeyVaultSecretsHealthCheck(SecretClient secretClient, AzureKeyVaultSecretOptions options)
    {
        _secretClient = Guard.ThrowIfNull(secretClient);
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string secretName = _options.SecretName;

        try
        {
            await _secretClient.GetSecretAsync(name: secretName, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new HealthCheckResult(HealthStatus.Healthy);
        }
        catch (RequestFailedException azureEx) when (azureEx.Status == 404) // based on https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/README.md#reporting-errors-requestfailedexception
        {
            if (_options.CreateWhenNotFound)
            {
                // When this call fails, the exception is caught by upper layer.
                // From https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks#create-health-checks:
                // "If CheckHealthAsync throws an exception during the check, a new HealthReportEntry is returned with its HealthReportEntry.Status set to the FailureStatus."
                await _secretClient.SetSecretAsync(name: secretName, value: secretName, cancellationToken).ConfigureAwait(false);
            }

            // The secret was not found, but it's fine as all we care about is whether it's possible to connect.
            return new HealthCheckResult(HealthStatus.Healthy);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
