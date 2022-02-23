using System;
using System.Collections.Generic;
using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NpgSqlHealthCheckBuilderExtensions
    {
        private const string NAME = "npgsql";

        /// <summary>
        /// Add a health check for Postgres databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="npgsqlConnectionString">The Postgres connection string to be used.</param>
        /// /// <param name="healthQuery">The query to be used in check. Optional. If <c>null</c> SELECT 1 is used.</param>
        /// <param name="connectionAction">An optional action to allow additional Npgsql-specific configuration.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddNpgSql(this IHealthChecksBuilder builder, string npgsqlConnectionString, string healthQuery = "SELECT 1;", Action<NpgsqlConnection> connectionAction = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.AddNpgSql(_ => npgsqlConnectionString, healthQuery, connectionAction, name, failureStatus, tags, timeout);
        }

        /// <summary>
        /// Add a health check for Postgres databases.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionStringFactory">A factory to build the Postgres connection string to use.</param>
        /// <param name="connectionStringFactory">The Postgres connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddNpgSql(this IHealthChecksBuilder builder, Func<IServiceProvider, string> connectionStringFactory, string healthQuery = "SELECT 1;", Action<NpgsqlConnection> connectionAction = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            if (connectionStringFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionStringFactory));
            }

            builder.Services.AddSingleton(sp => new NpgSqlHealthCheck(connectionStringFactory(sp), healthQuery, connectionAction));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<NpgSqlHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }
    }
}
