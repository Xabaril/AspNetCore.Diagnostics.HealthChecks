using HealthChecks.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignalRHealthCheckBuilderExtensions
    {
        const string NAME = "signalr";
        /// <summary>
        /// Add a health check for SignalR.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="url">The SignalR hub url to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'signalr' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddSignalRHub(this IHealthChecksBuilder builder, string url, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default,TimeSpan? timeout = default)
        {
            Func<HubConnection> hubConnectionBuilder = () => 
                new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();

            return builder.Add(
                new HealthCheckRegistration(
                    name ?? NAME,
                    sp => new SignalRHealthCheck(hubConnectionBuilder),
                    failureStatus,
                    tags,
                    timeout));
        }
        /// <summary>
        /// Add a health check for SignalR.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="hubConnectionBuilder">The SignalR hub connection builder to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'signalr' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddSignalRHub(this IHealthChecksBuilder builder, Func<HubConnection> hubConnectionBuilder, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default,TimeSpan? timeout = default)
        {
            return builder.Add(
                new HealthCheckRegistration(
                    name ?? NAME,
                    sp => new SignalRHealthCheck(hubConnectionBuilder),
                    failureStatus,
                    tags,
                    timeout));
        }
    }
}
