using System;
using System.Collections.Generic;
using HealthChecks.MySql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="MySqlHealthCheck"/>.
    /// </summary>
    public static class MySqlHealthCheckBuilderExtensions
    {
        private const string NAME = "mysql";

        /// <summary>
        /// Add a health check for MySql databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The MySql connection string to be used.</param>
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
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new MySqlHealthCheck(connectionString),
                failureStatus,
                tags,
                timeout));
        }
    }
}
