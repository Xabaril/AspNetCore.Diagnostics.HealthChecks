using HealthChecks.Nats;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="NatsHealthCheck"/>.
    /// </summary>
    public static class NatsHealthCheckBuilderExtensions
    {
        internal const string NAME = "nats";

        /// <summary>
        /// Add a health check for Nats.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Nats setup.</param>
        /// <param name="name">
        /// The health check name.
        /// Optional.
        /// If <see langword="null"/> the type name 'nats' will be used for the name.
        /// </param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails.
        /// Optional.
        /// If <see langword="null"/> then the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddNats(
            this IHealthChecksBuilder builder,
            Action<NatsOptions> setup,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            var options = new NatsOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new NatsHealthCheck(options),
                failureStatus,
                tags,
                timeout));
        }
    }
}
