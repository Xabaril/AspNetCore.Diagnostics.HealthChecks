using HealthChecks.Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HangfireHealthCheckBuilderExtensions
    {
        const string NAME_FAILED = "hangfire-failed";
        const string NAME_SERVERS = "hangfire-server";

        /// <summary>
        /// Add a health check for Hangfire
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Hangfire parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'hangfire-failed' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddHangfire(this IHealthChecksBuilder builder, Action<HangfireOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            var hangfireOptions = new HangfireOptions();
            setup?.Invoke(hangfireOptions);

            return builder.Add(new HealthCheckRegistration(
               name ?? NAME_FAILED,
               sp => new HangfireHealthCheck(hangfireOptions),
               failureStatus,
               tags));
        }

        /// <summary>
        /// Add a health check for Hangfire servers
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="minimumServers">Minimum of Hangfire servers</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'hangfire-servers' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddHangfireServers(this IHealthChecksBuilder builder, int minimumServers = 1, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? NAME_SERVERS,
               sp => new HangfireServerHealthCheck(minimumServers),
               failureStatus,
               tags));
        }
    }
}
