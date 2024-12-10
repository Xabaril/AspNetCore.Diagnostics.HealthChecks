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
    /// <param name="connectionString">The ClickHouse connection string to be used.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional ClickHouse specific configuration.</param>
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
        string connectionString,
        string healthQuery = HEALTH_QUERY,
        Action<ClickHouseConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddClickHouse(new ClickHouseHealthCheckOptions(connectionString)
        {
            CommandText = healthQuery,
            Configure = configure
        }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for ClickHouse databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the ClickHouse connection string to use.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional ClickHouse specific configuration.</param>
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
        Func<IServiceProvider, string> connectionStringFactory,
        string healthQuery = HEALTH_QUERY,
        Action<ClickHouseConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // This instance is captured in lambda closure, so it can be reused (perf)
        ClickHouseHealthCheckOptions options = new()
        {
            CommandText = healthQuery,
            Configure = configure,
        };

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                options.ConnectionString ??= Guard.ThrowIfNull(connectionStringFactory.Invoke(sp), throwOnEmptyString: true, paramName: nameof(connectionStringFactory));

                return new ClickHouseHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for ClickHouse databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">The ClickHouse connection factory to be used.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional ClickHouse specific configuration.</param>
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
        Func<ClickHouseConnection> connectionFactory,
        string healthQuery = HEALTH_QUERY,
        Action<ClickHouseConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddClickHouse(new ClickHouseHealthCheckOptions(connectionFactory)
        {
            CommandText = healthQuery,
            Configure = configure
        }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for ClickHouse databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">A factory to build the ClickHouse connection to use.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional ClickHouse specific configuration.</param>
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
        Action<ClickHouseConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        if (connectionFactory == null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        // This instance is captured in lambda closure, so it can be reused (perf)
        ClickHouseHealthCheckOptions options = new()
        {
            CommandText = healthQuery,
            Configure = configure,
        };

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                options.ConnectionFactory ??= () => Guard.ThrowIfNull(connectionFactory.Invoke(sp), paramName: nameof(connectionFactory));

                return new ClickHouseHealthCheck(options);
            },
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
