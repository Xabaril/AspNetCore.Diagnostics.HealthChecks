using Microsoft.Extensions.DependencyInjection;

namespace HealthCheks.Vault.DependencyInjection;

internal static class HealthCkecksVaultBuilderExtension
{
    internal static IServiceCollection AddVaultHealthCheck<TAuthMethodInfo>(
        this IServiceCollection services,
        string healthCheckName,
        Action<VaultHealthCheckOptions<TAuthMethodInfo>> configureOptions)
        where TAuthMethodInfo : IVaultAuthMethodInfo
    {
        var options = new VaultHealthCheckOptions<TAuthMethodInfo>();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddHealthChecks().AddCheck<HealthChecksVault<TAuthMethodInfo>>(healthCheckName);

        return services;
    }
}
