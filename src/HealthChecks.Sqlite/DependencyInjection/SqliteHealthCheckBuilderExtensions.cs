using HealthChecks.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="SqliteHealthCheck"/>.
    /// </summary>
    public static class SqliteHealthCheckBuilderExtensions
    {
        private const string NAME = "sqlite";
        internal const string HEALTH_QUERY = "select name from sqlite_master where type='table'";

        /// <summary>
        /// Add a health check for Sqlite databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Sqlite connection string to be used.</param>
        /// <param name="healthQuery">The query to be executed.</param>
        /// <param name="configure">An optional action to allow additional Sqlite specific configuration.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlite' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSqlite(
            this IHealthChecksBuilder builder,
            string connectionString,
            string healthQuery = HEALTH_QUERY,
            Action<SqliteConnection>? configure = null,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.AddOracle(_ => connectionString, healthQuery, configure, name, failureStatus, tags, timeout);
        }

        /// <summary>
        /// Add a health check for Sqlite databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionStringFactory">A factory to build the Sqlite connection string to be used.</param>
        /// <param name="healthQuery">The query to be used in check.</param>
        /// <param name="configure">An optional action to allow additional Sqlite specific configuration.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlite' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddOracle(
            this IHealthChecksBuilder builder,
            Func<IServiceProvider, string> connectionStringFactory,
            string healthQuery = HEALTH_QUERY,
            Action<SqliteConnection>? configure = null,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            Guard.ThrowIfNull(connectionStringFactory);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SqliteHealthCheck(new SqliteHealthCheckOptions
                {
                    ConnectionString = connectionStringFactory(sp),
                    CommandText = healthQuery,
                    Configure = configure,
                }),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for Sqlite databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="options">Options for health check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlite' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSqlite(
            this IHealthChecksBuilder builder,
            SqliteHealthCheckOptions options,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                _ => new SqliteHealthCheck(options),
                failureStatus,
                tags,
                timeout));
        }
    }
}
