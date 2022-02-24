using System;
using System.Collections.Generic;
using HealthChecks.Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="HangfireHealthCheck"/>.
    /// </summary>
    public static class HangfireHealthCheckBuilderExtensions
    {
        private const string NAME = "hangfire";

        /// <summary>
        /// Add a health check for Hangfire.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Hangfire parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'hangfire' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddHangfire(
            this IHealthChecksBuilder builder,
            Action<HangfireOptions>? setup,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            var hangfireOptions = new HangfireOptions();
            setup?.Invoke(hangfireOptions);

            return builder.Add(new HealthCheckRegistration(
               name ?? NAME,
               sp => new HangfireHealthCheck(hangfireOptions),
               failureStatus,
               tags,
               timeout));
        }
    }
}
