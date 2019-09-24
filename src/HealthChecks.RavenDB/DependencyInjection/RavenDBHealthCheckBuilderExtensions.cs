using HealthChecks.RavenDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RavenDBHealthCheckBuilderExtensions
    {
        const string NAME = "ravendb";
       
        /// <summary>
        /// Add a health check for RavenDB.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the RavenDB setup.</param>
        /// <param name="name">
        /// The health check name. Optional. If <see langword="null"/> the type name 'ravendb' will be used for the name.
        /// </param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddRavenDB(
            this IHealthChecksBuilder builder,
            Action<RavenDBOptions> setup,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            var options = new RavenDBOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new RavenDBHealthCheck(options),
                failureStatus,
                tags));
        }
    }
}
