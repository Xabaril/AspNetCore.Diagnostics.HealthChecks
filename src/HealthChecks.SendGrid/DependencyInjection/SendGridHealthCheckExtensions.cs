using System;
using System.Collections.Generic;
using System.Net.Http;
using HealthChecks.SendGrid;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SendGridHealthCheckExtensions
    {
        internal const string NAME = "sendgrid";

        /// <summary>
        /// Add a health check for SendGrid.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sendgrid' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddSendGrid(this IHealthChecksBuilder builder, string apiKey, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var registrationName = name ?? NAME;

            builder.Services.AddHttpClient(registrationName);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SendGridHealthCheck(apiKey, sp.GetRequiredService<IHttpClientFactory>()),
                failureStatus,
                tags,
                timeout));
        }
    }
}
