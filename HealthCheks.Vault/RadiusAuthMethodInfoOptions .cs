using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.RADIUS;

namespace HealthCheks.Vault;

internal class RadiusAuthMethodInfoOptions : IVaultAuthMethodInfo
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public IAuthMethodInfo GetAuthMethodInfo()
        => new RADIUSAuthMethodInfo(Username, Password);
}
