using HealthChecks.Vault;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VaultSharp;


namespace Microsoft.Extensions.DependencyInjection;


/// <summary>
/// Extension methods to configure vault services.
/// </summary>
public static class VaultHealthChecksBuilderExtension
{
    private const string NAME = "vault";

    /// <summary>
    /// Add a health check for vault services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="IVaultClient" /> instance.
    /// When not provided, <see cref="IVaultClient" /> is simply resolved from <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'vault' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddVault(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IVaultClient>? clientFactory = default,
        string? name = null,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new VaultHealthChecks(clientFactory?.Invoke(sp) ?? sp.GetRequiredService<IVaultClient>()),
           failureStatus,
           tags,
           timeout));
    }
}
