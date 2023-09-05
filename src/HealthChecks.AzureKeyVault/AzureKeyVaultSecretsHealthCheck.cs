using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureKeyVault;

/// <summary>
/// Azure Key Vault Secrets health check.
/// </summary>
public sealed class AzureKeyVaultSecretsHealthCheck : IHealthCheck
{
    private readonly SecretClient _secretClient;
    private readonly string _secretName;
    private readonly bool _createWhenNotFound;

    /// <summary>
    /// Creates new instance of Azure Key Vault Secrets health check.
    /// </summary>
    /// <param name="secretClient">
    /// The <see cref="SecretClient"/> used to perform Azure Key Vault operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="secretName">The name of the secret that will be fetched from Azure Key Vault. The default value is "AzureKeyVaultSecretsHealthCheck".</param>
    /// <param name="createWhenNotFound">
    /// A boolean value that indicates whether the secret should be created when it's not found.
    /// False by default. Enabling it requires secret set permissions and can be used to improve performance
    /// (secret not found is signaled via <see cref="RequestFailedException"/>).
    /// </param>
    /// <remarks>
    /// It uses the provided <paramref name="secretClient"/> to get given <paramref name="secretName"/> secret via
    /// <see cref="SecretClient.GetSecretAsync(string, string, CancellationToken)"/> method.
    /// When the secret is not found, it will try to create it if <paramref name="createWhenNotFound"/> is set to true.
    /// When the secret is not found, but <paramref name="createWhenNotFound"/> is false, it returns <see cref="HealthStatus.Healthy"/> status,
    /// as the connection to the service itself can be made.
    /// </remarks>
    public AzureKeyVaultSecretsHealthCheck(SecretClient secretClient, string secretName = nameof(AzureKeyVaultSecretsHealthCheck), bool createWhenNotFound = false)
    {
        _secretClient = Guard.ThrowIfNull(secretClient);
        _secretName = Guard.ThrowIfNull(secretName, throwOnEmptyString: true);
        _createWhenNotFound = createWhenNotFound;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _secretClient.GetSecretAsync(name: _secretName, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new HealthCheckResult(HealthStatus.Healthy);
        }
        catch (RequestFailedException azureEx) when (azureEx.Status == 404) // based on https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/README.md#reporting-errors-requestfailedexception
        {
            if (_createWhenNotFound)
            {
                // When this call fails, the exception is caught by upper layer.
                // From https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks#create-health-checks:
                // "If CheckHealthAsync throws an exception during the check, a new HealthReportEntry is returned with its HealthReportEntry.Status set to the FailureStatus."
                await _secretClient.SetSecretAsync(name: _secretName, value: _secretName, cancellationToken).ConfigureAwait(false);
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
