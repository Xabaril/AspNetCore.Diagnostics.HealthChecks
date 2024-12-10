using ClickHouse.Client.ADO;
using HealthChecks.ClickHouse;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ClickHouseHealthCheck"/>.
/// </summary>
public static class ClickHouseHealthCheckBuilderExtensions
{
    private const string NAME = "ClickHouse";
    internal const string HEALTH_QUERY = "SELECT 1;";

    /// <summary>
    /// Add a health check for ClickHouse databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">A factory to build the ClickHouse connection to use.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ClickHouse' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddClickHouse(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, ClickHouseConnection> connectionFactory,
        string healthQuery = HEALTH_QUERY,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        if (connectionFactory == null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ClickHouseHealthCheck(new ClickHouseHealthCheckOptions(() => Guard.ThrowIfNull(connectionFactory.Invoke(sp), paramName: nameof(connectionFactory)))
            {
                CommandText = healthQuery
            }),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for ClickHouse databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ClickHouse' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddClickHouse(
        this IHealthChecksBuilder builder,
        ClickHouseHealthCheckOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new ClickHouseHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
