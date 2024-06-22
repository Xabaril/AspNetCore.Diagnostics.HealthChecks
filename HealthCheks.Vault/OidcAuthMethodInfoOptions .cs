using VaultSharp.V1.AuthMethods;

namespace HealthCheks.Vault;

public class OidcAuthMethodInfoOptions : IVaultAuthMethodInfo
{
    public string? Token { get; set; }

    public IAuthMethodInfo GetAuthMethodInfo()
        => throw new NotImplementedException();
}
