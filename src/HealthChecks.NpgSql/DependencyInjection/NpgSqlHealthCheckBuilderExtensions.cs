using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="NpgSqlHealthCheck"/>.
/// </summary>
public static class NpgSqlHealthCheckBuilderExtensions
{
    private const string NAME = "npgsql";
    internal const string HEALTH_QUERY = "SELECT 1;";

    /// <summary>
    /// Add a health check for Postgres databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Postgres connection string to be used.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional Npgsql specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNpgSql(
        this IHealthChecksBuilder builder,
        string connectionString,
        string healthQuery = HEALTH_QUERY,
        Action<NpgsqlConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddNpgSql(new NpgSqlHealthCheckOptions(connectionString)
        {
            CommandText = healthQuery,
            Configure = configure
        }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Postgres databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Postgres connection string to use.</param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional Npgsql specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNpgSql(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string healthQuery = HEALTH_QUERY,
        Action<NpgsqlConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // This instance is captured in lambda closure, so it can be reused (perf)
        NpgSqlHealthCheckOptions options = new()
        {
            CommandText = healthQuery,
            Configure = configure,
        };

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                options.ConnectionString ??= Guard.ThrowIfNull(connectionStringFactory.Invoke(sp), throwOnEmptyString: true, paramName: nameof(connectionStringFactory));

                ResolveDataSourceIfPossible(options, sp);

                return new NpgSqlHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Postgres databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="dbDataSourceFactory">
    /// An optional factory to obtain <see cref="NpgsqlDataSource" /> instance.
    /// When not provided, <see cref="NpgsqlDataSource" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="healthQuery">The query to be used in check.</param>
    /// <param name="configure">An optional action to allow additional Npgsql specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    /// <remarks>
    /// Depending on how the <see cref="NpgsqlDataSource" /> was configured, the connections it hands out may be pooled.
    /// That is why it should be the exact same <see cref="NpgsqlDataSource" /> that is used by other parts of your app.
    /// </remarks>
    public static IHealthChecksBuilder AddNpgSql(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, NpgsqlDataSource>? dbDataSourceFactory = null,
        string healthQuery = HEALTH_QUERY,
        Action<NpgsqlConnection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // This instance is captured in lambda closure, so it can be reused (perf)
        NpgSqlHealthCheckOptions options = new()
        {
            CommandText = healthQuery,
            Configure = configure,
        };

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                options.DataSource ??= dbDataSourceFactory?.Invoke(sp) ?? sp.GetRequiredService<NpgsqlDataSource>();

                return new NpgSqlHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Postgres databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check. It's mandatory to provide <see cref="NpgSqlHealthCheckOptions.ConnectionString"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNpgSql(
        this IHealthChecksBuilder builder,
        NpgSqlHealthCheckOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                ResolveDataSourceIfPossible(options, sp);

                return new NpgSqlHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    private static void ResolveDataSourceIfPossible(NpgSqlHealthCheckOptions options, IServiceProvider sp)
    {
        if (options.DataSource is null && !options.TriedToResolveFromDI)
        {
            NpgsqlDataSource? fromDi = sp.GetService<NpgsqlDataSource>();
            if (fromDi?.ConnectionString == options.ConnectionString)
            {
                // When it's possible, we reuse the DataSource registered in the DI.
                // We do that to achieve best performance and avoid issues like https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/1993
                options.DataSource = fromDi;
            }
            options.TriedToResolveFromDI = true; // save the answer, so we don't do it more than once
        }
    }
}
