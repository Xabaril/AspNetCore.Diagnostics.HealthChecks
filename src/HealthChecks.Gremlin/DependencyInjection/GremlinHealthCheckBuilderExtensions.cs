using HealthChecks.Gremlin;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GremlinHealthCheckBuilderExtensions
    {
        const string NAME = "gremlin";

        /// <summary>
        /// Add a health check for Apache TinkerPop Gremlin database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionOptionsFactory">A factory to build the Gremlin connection data to use.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'gremlin' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddGremlin(this IHealthChecksBuilder builder,
            Func<IServiceProvider, GremlinOptions> connectionOptionsFactory,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            _ = connectionOptionsFactory ?? throw new ArgumentNullException(nameof(connectionOptionsFactory));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new GremlinHealthCheck(connectionOptionsFactory(sp)),
                failureStatus,
                tags,
                timeout));
        }
    }
}
