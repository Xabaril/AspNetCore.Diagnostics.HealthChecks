using HealthChecks.Aws.SystemsManager;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="SystemsManagerHealthCheck"/>.
    /// </summary>
    public static class SystemsManagerHealthCheckBuilderExtensions
    {
        private const string NAME = "aws systems manager";

        /// <summary>
        /// Add a health check for AWS Systems Manager.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Systems Manager Configuration e.g. access key, secret key, region etc. </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'aws systems manager' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSystemsManager(
            this IHealthChecksBuilder builder,
            Action<SystemsManagerOptions>? setup,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            var options = new SystemsManagerOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SystemsManagerHealthCheck(options),
                failureStatus,
                tags,
                timeout));
        }
    }
}
