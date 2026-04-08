using Azure.Security.KeyVault.Certificates;
using HealthChecks.Azure.KeyVault.Certificates;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class AzureKeyVaultCertificatesHealthChecksBuilderExtensions
{
    private const string NAME = "azure_key_vault_certificate";

    public static IHealthChecksBuilder AddAzureKeyVaultCertificates(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, CertificateClient>? clientFactory = default,
        Func<IServiceProvider, AzureKeyVaultCertificatesHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new AzureKeyVaultCertificatesHealthCheck(
                certificateClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<CertificateClient>(),
                options: optionsFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
