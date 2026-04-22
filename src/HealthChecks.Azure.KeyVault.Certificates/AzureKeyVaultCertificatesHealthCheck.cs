using Azure;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.KeyVault.Certificates;

public sealed class AzureKeyVaultCertificatesHealthCheck : IHealthCheck
{
    private readonly CertificateClient _certificateClient;
    private readonly AzureKeyVaultCertificatesHealthCheckOptions _options;

    public AzureKeyVaultCertificatesHealthCheck(CertificateClient certificateClient, AzureKeyVaultCertificatesHealthCheckOptions? options = default)
    {
        _certificateClient = Guard.ThrowIfNull(certificateClient);
        _options = options ?? new AzureKeyVaultCertificatesHealthCheckOptions();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string certificateName = _options.CertificateName;

        try
        {
            await _certificateClient.GetCertificateAsync(certificateName, cancellationToken).ConfigureAwait(false);
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
