using Azure.Data.Tables;
using HealthChecks.Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureTableServiceHealthCheck"/>.
/// </summary>
public static class AzureTablesServiceHealthChecksBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "azure_tables";

    /// <summary>
    /// Add a health check for Azure Tables Service by registering <see cref="AzureTableServiceHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="TableServiceClient" /> instance.
    /// When not provided, <see cref="TableServiceClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="AzureTableServiceHealthCheckOptions"/> used by the health check.
    /// When not provided, defaults are used.
    /// </param>
    /// <param name="healthCheckName">The health check name. Optional. If <c>null</c> the name 'azure_tables' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureTable(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, TableServiceClient>? clientFactory = default,
        Func<IServiceProvider, AzureTableServiceHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = HEALTH_CHECK_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           string.IsNullOrEmpty(healthCheckName) ? HEALTH_CHECK_NAME : healthCheckName!,
           sp => new AzureTableServiceHealthCheck(
                    tableServiceClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<TableServiceClient>(),
                    options: optionsFactory?.Invoke(sp)),
           failureStatus,
           tags,
           timeout));
    }
}
