using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using HealthChecks.ArangoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ArangoDbHealthCheckBuilderExtensions
    {
        private const string NAME = "arangodb";

        /// <summary>
        /// Add a health check for ArangoDB.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionOptionsFactory">A factory to build the ArangoDB connection data to use.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'arangodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddArangoDb(this IHealthChecksBuilder builder,
            Func<IServiceProvider, ArangoDbOptions> connectionOptionsFactory,
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
                sp => new ArangoDbHealthCheck(connectionOptionsFactory(sp)), 
                failureStatus,
                tags,
                timeout));
        }
    }
}
