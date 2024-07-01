namespace HealthCheks.Vault;

public class VaultHealthCheckOptions
{
    // Vault address, e.g., "http://127.0.0.1:8200"
    public string? VaultAddress { get; set; } = null;
    //AuthenticationType
    public VaultCheckAuthenticationType AuthenticationType { get; private set; }
    //radius 
    public string? RadiusUserName { get; private set; }
    public string? RadiusPassword { get; private set; }
    //okta 
    public string? OktaUsername { get; private set; }
    public string? OktaPassword { get; private set; }
    //basic vault token
    public string? BasicVaultToken { get; private set; }
    //lpad 
    public string? LdapUserName { get; private set; }
    public string? LdapPassword { get; private set; }

    //check address
    public VaultHealthCheckOptions WithVaultAddress(string vaultAddress)
    {
        VaultAddress = vaultAddress ?? throw new ArgumentNullException(nameof(vaultAddress));
        return this;
    }


    //Authentication Methods
    public VaultHealthCheckOptions UseBasicAuthentication(string basicVauldToken)
    {
        BasicVaultToken = basicVauldToken
            ?? throw new ArgumentNullException(nameof(basicVauldToken));
        AuthenticationType = VaultCheckAuthenticationType.IsBasicVaultAuthentication;

        RadiusPassword = string.Empty;
        RadiusUserName = string.Empty;
        OktaUsername = string.Empty;
        OktaPassword = string.Empty;
        LdapPassword = string.Empty;
        LdapUserName = string.Empty;

        return this;
    }


    public VaultHealthCheckOptions UseRadiusAuthentication(string radiusPassword, string radiusUserName)
    {
        RadiusPassword = radiusPassword
           ?? throw new ArgumentNullException(nameof(radiusPassword));
        RadiusUserName = radiusUserName
          ?? throw new ArgumentNullException(nameof(radiusUserName));
        AuthenticationType = VaultCheckAuthenticationType.IsRadiusAuthentication;

        OktaUsername = string.Empty;
        OktaPassword = string.Empty;
        LdapPassword = string.Empty;
        BasicVaultToken = string.Empty;
        LdapUserName = string.Empty;

        return this;
    }

    public VaultHealthCheckOptions UseOktaAuthentication(string oktaPassword, string oktaUserName)
    {
        OktaUsername = oktaUserName
            ?? throw new ArgumentNullException(nameof(oktaUserName));
        OktaPassword = oktaPassword
            ?? throw new ArgumentNullException(nameof(oktaPassword));
        AuthenticationType = VaultCheckAuthenticationType.IsOktaAuthentication;

        RadiusPassword = string.Empty;
        RadiusUserName = string.Empty;
        BasicVaultToken = string.Empty;
        LdapPassword = string.Empty;
        LdapUserName = string.Empty;

        return this;
    }


    public VaultHealthCheckOptions UseLdapAuthentication(string lpadPassword, string lpaduserName)
    {
        LdapPassword = lpadPassword
             ?? throw new ArgumentNullException(nameof(lpadPassword));
        LdapUserName = lpaduserName
            ?? throw new ArgumentNullException(nameof(lpaduserName));
        AuthenticationType = VaultCheckAuthenticationType.IsLdapAuthentication;

        RadiusPassword = string.Empty;
        RadiusUserName = string.Empty;
        OktaUsername = string.Empty;
        OktaPassword = string.Empty;
        BasicVaultToken = string.Empty;

        return this;
    }

}
public enum VaultCheckAuthenticationType
{
    IsBasicVaultAuthentication = 1,
    IsRadiusAuthentication,
    IsLdapAuthentication,
    IsOktaAuthentication
}
