using HealthChecks.Azure.KeyVault.Secrets;

namespace HealthChecks.AzureKeyVault.Tests.Functional;

public class AzureKeyVaultSecretOptionsTests
{
    [Fact]
    public void SecretNameThrowsArgumentNullExceptionForNull()
    {
        AzureKeyVaultSecretOptions sut = new();
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(() => sut.SecretName = null!);
        Assert.Equal("SecretName", argumentNullException.ParamName);
    }

    [Fact]
    public void SecretNameThrowsArgumentNullExceptionForEmptyString()
    {
        AzureKeyVaultSecretOptions sut = new();
        ArgumentException argumentException = Assert.ThrowsAny<ArgumentException>(() => sut.SecretName = string.Empty);
        Assert.Equal("SecretName", argumentException.ParamName);
    }
}
