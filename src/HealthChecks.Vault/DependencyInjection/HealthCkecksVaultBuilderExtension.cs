using HealthCheks.Vault;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Adds a Vault health check to the health check builder.
/// </summary>

public static class HealthCkecksVaultBuilderExtension
{
    private const string NAME = "HashicorpVault";
    /// <summary>
    /// Adds a Vault health check to the health check builder.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to which the Vault health check will be added.</param>
    /// <param name="vaultAddress">The address of the Vault server.</param>
    /// <param name="basicVaultToken">The basic authentication token used to authenticate with the Vault server.</param>
    /// <param name="name">The name of the health check. This will be used in health check responses.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> to report when the health check fails. If null, the default failure status is used.
    /// </param>
    /// <param name="tags">Optional tags that can be used to filter health checks.</param>
    /// <param name="timeout">
    /// The timeout for the health check. If null, the default timeout is used.
    /// </param>
    /// <returns>The updated <see cref="IHealthChecksBuilder"/> with the Vault health check added.</returns>
    /// <remarks>
    /// This method configures a Vault health check by setting up basic authentication and the Vault server address.
    /// The health check will use the provided <paramref name="name"/> to identify itself, and optional parameters 
    /// like <paramref name="tags"/>, <paramref name="failureStatus"/>, and <paramref name="timeout"/> allow further customization.
    /// </remarks>
    public static IHealthCheckBuilder AddVault(
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
           name ?? NAME,
           sp => new HealthChecks.Vault.HealthChecksVault(options),
           failureStatus,
           tags,
           timeout));
    }
}
