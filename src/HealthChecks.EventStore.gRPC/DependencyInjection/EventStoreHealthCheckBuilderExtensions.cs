using HealthChecks.EventStore.gRPC;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="EventStoreHealthCheck"/>.
    /// </summary>
    public static class EventStoreHealthCheckBuilderExtensions
    {
        private const string NAME = "eventstore";

        /// <summary>
        /// Add a health check for EventStore services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The EventStore connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'eventstore' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddEventStore(
            this IHealthChecksBuilder builder,
            string connectionString,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new EventStoreHealthCheck(connectionString),
                failureStatus,
                tags,
                timeout));
        }
    }
}
