using HealthChecks.EventStore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventStoreHealthCheckBuilderExtensions
    {
        const string NAME = "eventstore";
        /// <summary>
        /// Add a health check for EventStore services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="eventStoreConnectionUri">The EventStore connection URI to be used.</param>
        /// <param name="login">The EventStore user login. Optional. If <c>null</c> the healthcheck will be processed without authentication.</param>
        /// <param name="password">The EventStore user password. Optional. If <c>null</c> the healthcheck will be processed without authentication.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'eventstore' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddEventStore(this IHealthChecksBuilder builder, string eventStoreConnectionUri, string login = default, string password = default, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new EventStoreHealthCheck(eventStoreConnectionUri, login, password),
                failureStatus,
                tags));
        }
        /// <summary>
        /// Add a health check for EventStore services using Connection String.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="eventStoreConnectionString">The EventStore Connection String to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'eventstore' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddEventStoreConnectionString(this IHealthChecksBuilder builder, string eventStoreConnectionString, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new EventStoreConnectionStringHealthCheck(eventStoreConnectionString),
                failureStatus,
                tags));
        }
    }
}
