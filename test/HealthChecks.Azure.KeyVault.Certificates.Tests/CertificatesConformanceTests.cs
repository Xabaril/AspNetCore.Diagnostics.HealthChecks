using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using HealthChecks.Azure.KeyVault.Certificates;

namespace HealthChecks.Azure.KeyVault.Certificates.Tests;

public class CertificatesConformanceTests : ConformanceTests<CertificateClient, AzureKeyVaultCertificatesHealthCheck, AzureKeyVaultCertificatesHealthCheckOptions>
{
    protected override CertificateClient CreateClientForNonExistingEndpoint()
    {
        CertificateClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureKeyVaultCertificatesHealthCheckOptions CreateHealthCheckOptions() => new();

    protected override AzureKeyVaultCertificatesHealthCheck CreateHealthCheck(CertificateClient client, AzureKeyVaultCertificatesHealthCheckOptions? options)
        => new(client, options);

    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, CertificateClient>? clientFactory = null, Func<IServiceProvider, AzureKeyVaultCertificatesHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureKeyVaultCertificates(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
}
