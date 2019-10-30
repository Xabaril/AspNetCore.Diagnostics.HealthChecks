using HealthChecks.CosmosDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosDbHealthCheckBuilderExtensions
    {
        const string NAME = "cosmosdb";
        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">CosmosDb full connection string.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddCosmosDb(this IHealthChecksBuilder builder, string connectionString, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? NAME,
               sp => new CosmosDbHealthCheck(connectionString),
               failureStatus,
               tags,
               timeout));
        }
    }
}
