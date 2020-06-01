using ClickHouse.Ado;
using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClickhouseHealthCheckBuilderExtensions
    {
        const string NAME = "clickhouse";

        /// <summary>
        /// Add a health check for Clickhouse databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="clickhouseConnectionString">The Clickhouse connection string to be used.</param>
        /// /// <param name="healthQuery">The query to be used in check. Optional. If <c>null</c> SELECT 1 is used.</param>
        /// <param name="connectionAction">An optional action to allow additional Clickhouse-specific configuration.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'clickhouse' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddNpgSql(this IHealthChecksBuilder builder, string clickhouseConnectionString, string healthQuery = "SELECT 1;", Action<ClickHouseConnection> connectionAction = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new ClickhouseHealthCheck(clickhouseConnectionString, healthQuery, connectionAction),
                failureStatus,
                tags,
                timeout));
        }
    }
}
