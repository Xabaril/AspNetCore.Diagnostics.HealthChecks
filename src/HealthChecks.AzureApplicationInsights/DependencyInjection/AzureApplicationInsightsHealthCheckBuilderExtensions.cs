using HealthChecks.AzureApplicationInsights;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureApplicationInsightsHealthCheckBuilderExtensions
    {
        const string AZUREAPPLICATIONINSIGHTS_NAME = "azureappinsights";

        /// <summary>
        /// Add a health check for specified Azure Event Hub.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="instrumentationKey">The azure app insights instrumentation ky.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureApplicationInsights(this IHealthChecksBuilder builder, string instrumentationKey, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
              return builder.Add(new HealthCheckRegistration(
                  name ?? AZUREAPPLICATIONINSIGHTS_NAME,
                  sp => new AzureApplicationInsightsHealthCheck(instrumentationKey),
                  failureStatus,
                  tags,
                  timeout));
        }
    }
}
