using HealthChecks.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqliteHealthCheckBuilderExtensions
    {
        const string NAME = "sqlite";

        /// <summary>
        /// Add a health check for Sqlite services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="sqliteConnectionString">The Sqlite connection string to be used.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select name from sqlite_master where type='table' is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlite' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">The timeout after which the health check is considered failed. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddSqlite(this IHealthChecksBuilder builder, string sqliteConnectionString, string healthQuery = "select name from sqlite_master where type='table'", string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SqliteHealthCheck(sqliteConnectionString, healthQuery, timeout ?? Timeout.InfiniteTimeSpan),
                failureStatus,
                tags));
        }
    }
}
