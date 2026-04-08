namespace HealthChecks.Azure.KeyVault.Certificates;

public sealed class AzureKeyVaultCertificatesHealthCheckOptions
{
    private string _certificateName = nameof(AzureKeyVaultCertificatesHealthCheck);

    public string CertificateName
    {
        get => _certificateName;
        set => _certificateName = Guard.ThrowIfNull(value, throwOnEmptyString: true, paramName: nameof(CertificateName));
    }
}
