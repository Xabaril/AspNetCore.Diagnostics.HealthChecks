using VaultSharp.V1.AuthMethods.Okta;
using VaultSharp.V1.AuthMethods;

namespace HealthCheks.Vault;

public class OktaAuthMethodInfoOptions : IVaultAuthMethodInfo
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public IAuthMethodInfo GetAuthMethodInfo()
        => new OktaAuthMethodInfo(Username, Password);
}
