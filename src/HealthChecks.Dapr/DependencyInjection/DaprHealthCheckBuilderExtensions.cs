using Dapr.Client;
using HealthChecks.Dapr;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DaprHealthCheckBuilderExtensions
    {
        private const string DAPR_NAME = "dapr";

        /// <summary>
        /// Add a health check for Dapr using provided <see cref="DaprClient"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="daprClient">The DaprClient to be used. <see cref="DaprClient"/>/param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'dapr' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddDapr(
            this IHealthChecksBuilder builder,
            DaprClient daprClient,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? DAPR_NAME,
                sp => new DaprHealthCheck(daprClient),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for Dapr using <see cref="DaprClient"/> from service provider 
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'dapr' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddDapr(
            this IHealthChecksBuilder builder,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? DAPR_NAME,
                sp => new DaprHealthCheck(sp.GetRequiredService<DaprClient>()),
                failureStatus,
                tags,
                timeout));
        }
    }
}
