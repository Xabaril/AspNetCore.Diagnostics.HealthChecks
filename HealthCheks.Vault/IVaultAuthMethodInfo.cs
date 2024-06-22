using VaultSharp.V1.AuthMethods;

namespace HealthCheks.Vault;

internal interface IVaultAuthMethodInfo
{
    IAuthMethodInfo GetAuthMethodInfo();
}
