using HealthChecks.Azure.KeyVault.Keys;

namespace HealthChecks.Azure.KeyVault.Keys.Tests.Functional;

public class AzureKeyVaultKeyOptionsTests
{
    [Fact]
    public void KeyNameThrowsArgumentNullExceptionForNull()
    {
        AzureKeyVaultKeysHealthCheckOptions sut = new();
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(() => sut.KeyName = null!);
        Assert.Equal("KeyName", argumentNullException.ParamName);
    }

    [Fact]
    public void KeyNameThrowsArgumentNullExceptionForEmptyString()
    {
        AzureKeyVaultKeysHealthCheckOptions sut = new();
        ArgumentException argumentException = Assert.ThrowsAny<ArgumentException>(() => sut.KeyName = string.Empty);
        Assert.Equal("KeyName", argumentException.ParamName);
    }
}
