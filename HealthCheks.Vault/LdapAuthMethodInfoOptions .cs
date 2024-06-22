using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.LDAP;

namespace HealthCheks.Vault;

internal class LdapAuthMethodInfoOptions : IVaultAuthMethodInfo
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public IAuthMethodInfo GetAuthMethodInfo()
        => new LDAPAuthMethodInfo(Username, Password);
}
