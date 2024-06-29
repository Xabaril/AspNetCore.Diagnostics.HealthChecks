using Microsoft.Extensions.Diagnostics.HealthChecks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.LDAP;
using VaultSharp.V1.AuthMethods.Okta;
using VaultSharp.V1.AuthMethods.RADIUS;
using VaultSharp.V1.AuthMethods.Token;

namespace HealthCheks.Vault;


public class HealthChecksVault : IHealthCheck
{
    private readonly VaultHealthCheckOptions _options;
    private IVaultClient? _vaultClient;

    public HealthChecksVault(VaultHealthCheckOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        InitializeVaultClient();
    }

    //initialized vault
    private void InitializeVaultClient()
    {
        IAuthMethodInfo? AuthMethodInfo = null;
        switch (_options.AuthenticationType)
        {
            case VaultCheckAuthenticationType.IsBasicVaultAuthentication:
                AuthMethodInfo = new TokenAuthMethodInfo(vaultToken: _options.BasicVaultToken);
                break;
            case VaultCheckAuthenticationType.IsRadiusAuthentication:
                AuthMethodInfo = new RADIUSAuthMethodInfo(_options.RadiusUserName, _options.RadiusPassword);
                break;
            case VaultCheckAuthenticationType.IsLdapAuthentication:
                AuthMethodInfo = new LDAPAuthMethodInfo(_options.LdapUserName, _options.LdapPassword);
                break;
            case VaultCheckAuthenticationType.IsOktaAuthentication:
                AuthMethodInfo = new OktaAuthMethodInfo(_options.OktaUsername, _options.OktaPassword);
                break;
            default:
                throw new InvalidOperationException("can not resolve correct parameter AuthMethodInfo");
        }
        var vaultClientSettings = new VaultClientSettings(_options.VaultAddress, AuthMethodInfo);
        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await _vaultClient!.V1.System.GetHealthStatusAsync().ConfigureAwait(false);

            if (healthStatus.Initialized && healthStatus.Sealed)
                return new HealthCheckResult(context.Registration.FailureStatus, description: "Vault is initialized but sealed.");
            else if (!healthStatus.Initialized)
                return new HealthCheckResult(context.Registration.FailureStatus, description: "Vault is not initialized.");

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}

