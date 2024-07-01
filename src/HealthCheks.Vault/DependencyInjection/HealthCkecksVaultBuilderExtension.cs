using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheks.Vault.DependencyInjection;

public static class HealthCkecksVaultBuilderExtension
{
    public static IHealthChecksBuilder AddVaultHealthCheck(
        this IHealthChecksBuilder builder,
        string vaultAddress,
        string basicVaultToken,
        string name,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new VaultHealthCheckOptions();
        options.UseBasicAuthentication(basicVaultToken)
            .WithVaultAddress(vaultAddress);

        return builder.Add(new HealthCheckRegistration(
           name,
           sp => new HealthChecksVault(options),
           failureStatus,
           tags,
           timeout));
    }

    public static IHealthChecksBuilder AddVaultHealthCheck(
       this IHealthChecksBuilder builder,
       string vaultAddress,
       string userName,
       string Password,
       VaultCheckAuthenticationType authenticationType,
       string name,
       HealthStatus? failureStatus = default,
       IEnumerable<string>? tags = default,
       TimeSpan? timeout = default)
    {
        var options = new VaultHealthCheckOptions();
        switch (authenticationType)
        {
            case VaultCheckAuthenticationType.IsRadiusAuthentication:
                options.UseRadiusAuthentication(Password, userName)
                    .WithVaultAddress(vaultAddress);
                break;
            case VaultCheckAuthenticationType.IsLdapAuthentication:
                options.UseLdapAuthentication(Password, userName)
                    .WithVaultAddress(vaultAddress);
                break;
            case VaultCheckAuthenticationType.IsOktaAuthentication:
                options.UseOktaAuthentication(Password, userName)
                    .WithVaultAddress(vaultAddress);
                break;
            default:
                throw new ArgumentException("Invalid parameter");
        }


        return builder.Add(new HealthCheckRegistration(
           name,
           sp => new HealthChecksVault(options),
           failureStatus,
           tags,
           timeout));
    }

}
