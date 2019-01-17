using HealthChecks.Consul;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConsulHealthCheckBuilderExtensions
    {
        private const string NAME = "consul";

        /// <summary>
        /// Add a health check for Consul services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="consulHost">The hostname where consulServer is located</param>
        /// <param name="consulPort">The Port where consulServer is located</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'consul' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddConsul(this IHealthChecksBuilder builder, string consulHost, int consulPort, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new ConsulHealthCheck(consulHost, consulPort),
                failureStatus,
                tags));
        }
    }
}