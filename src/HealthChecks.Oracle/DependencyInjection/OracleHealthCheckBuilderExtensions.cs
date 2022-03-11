using HealthChecks.Oracle;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="OracleHealthCheck"/>.
    /// </summary>
    public static class OracleHealthCheckBuilderExtensions
    {
        private const string NAME = "oracle";
        private const string HEALTH_QUERY = "select * from v$version";

        /// <summary>
        /// Add a health check for Oracle databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Oracle connection string to be used.</param>
        /// <param name="healthQuery">The query to be used in check. Optional. If <c>null</c> select * from v$version is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'oracle' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddOracle(
            this IHealthChecksBuilder builder,
            string connectionString,
            string healthQuery = HEALTH_QUERY,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default) => builder.AddOracle(_ => connectionString, healthQuery, name, failureStatus, tags, timeout);

        /// <summary>
        /// Add a health check for Oracle databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionStringFactory">A factory to build the Oracle connection string to be used.</param>
        /// <param name="healthQuery">The query to be used in check. Optional. If <c>null</c> select * from v$version is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'oracle' will be used for the name.</param>
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
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            if (connectionStringFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionStringFactory));
            }

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new OracleHealthCheck(connectionStringFactory(sp), healthQuery),
                failureStatus,
                tags,
                timeout));
        }
    }
}
