using HealthChecks.Azure.KeyVault.Certificates;

namespace HealthChecks.Azure.KeyVault.Certificates.Tests.Functional;

public class AzureKeyVaultCertificateOptionsTests
{
    [Fact]
    public void CertificateNameThrowsArgumentNullExceptionForNull()
    {
        AzureKeyVaultCertificatesHealthCheckOptions sut = new();
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(() => sut.CertificateName = null!);
        Assert.Equal("CertificateName", argumentNullException.ParamName);
    }

    [Fact]
    public void CertificateNameThrowsArgumentNullExceptionForEmptyString()
    {
        AzureKeyVaultCertificatesHealthCheckOptions sut = new();
        ArgumentException argumentException = Assert.ThrowsAny<ArgumentException>(() => sut.CertificateName = string.Empty);
        Assert.Equal("CertificateName", argumentException.ParamName);
    }
}
