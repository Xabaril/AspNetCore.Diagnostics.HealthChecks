using HealthChecks.MySql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="MySqlHealthCheck"/>.
/// </summary>
public static class MySqlHealthCheckBuilderExtensions
{
    private const string NAME = "mysql";

    /// <summary>
    /// Add a health check for MySQL databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="dataSourceFactory">An optional factory to create the <see cref="MySqlDataSource"/>. By default, one will be retrieved from the service collection.</param>
    /// <param name="healthQuery">The optional query to be executed. If this is <c>null</c>, a MySQL "ping" packet will be sent to the server instead of a query.</param>
    /// <param name="configure">An optional action to allow additional MySQL specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mysql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMySql(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, MySqlDataSource>? dataSourceFactory = null,
        string? healthQuery = null,
        Action<MySqlConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new MySqlHealthCheck(new(dataSourceFactory?.Invoke(sp) ?? sp.GetRequiredService<MySqlDataSource>())
            {
                CommandText = healthQuery,
                Configure = configure,
            }),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for MySQL databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The MySQL connection string to be used.</param>
    /// <param name="healthQuery">The optional query to be executed. If this is <c>null</c>, a MySQL "ping" packet will be sent to the server instead of a query.</param>
    /// <param name="configure">An optional action to allow additional MySQL specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mysql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMySql(
        this IHealthChecksBuilder builder,
        string connectionString,
        string? healthQuery = null,
        Action<MySqlConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddMySql(new(connectionString)
        {
            CommandText = healthQuery,
            Configure = configure
        }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for MySQL databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the MySQL connection string to use.</param>
    /// <param name="healthQuery">The optional query to be executed. If this is <c>null</c>, a MySQL "ping" packet will be sent to the server instead of a query.</param>
    /// <param name="configure">An optional action to allow additional MySQL specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mysql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMySql(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string? healthQuery = null,
        Action<MySqlConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new MySqlHealthCheck(new(connectionStringFactory(sp))
            {
                CommandText = healthQuery,
                Configure = configure,
            }),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for MySQL databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mysql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMySql(
        this IHealthChecksBuilder builder,
        MySqlHealthCheckOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new MySqlHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
