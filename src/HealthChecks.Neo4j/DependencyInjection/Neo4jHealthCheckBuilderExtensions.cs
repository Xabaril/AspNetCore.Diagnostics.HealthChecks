using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using HealthChecks.Neo4j;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Neo4jHealthCheckBuilderExtensions
    {
        const string NAME = "neo4j";

        /// <summary>
        /// Add a health check for Neo4j services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionOptionsFactory">A factory to build the Neo4j connection data to use.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'neo4j' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddNeo4j(this IHealthChecksBuilder builder,
            Func<IServiceProvider, Neo4jOptions> connectionOptionsFactory,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            if (connectionOptionsFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionOptionsFactory));
            }

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new Neo4jHealthCheck(connectionOptionsFactory(sp)),
                failureStatus,
                tags,
                timeout));
        }
    }
}