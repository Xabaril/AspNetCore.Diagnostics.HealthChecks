using HealthChecks.AzureIoTHub;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureIoTHubHealthChecksBuilderExtensions
    {
        const string NAME = "azureiothub";

        /// <summary>
        /// Add a health check for Azure IoT Hub.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="optionsFactory">A factory to build the Azure IoT Hub connection and health check options to use.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureiothub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureIoTHub(this IHealthChecksBuilder builder,
            Func<IServiceProvider, AzureIoTHubOptions> optionsFactory,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            if (optionsFactory == null)
            {
                throw new ArgumentNullException(nameof(optionsFactory));
            }
            
            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
               registrationName,
               sp =>
               {
                   var options = optionsFactory(sp);
                   return new AzureIoTHubHealthCheck(options);
               },
               failureStatus,
               tags,
               timeout));
        }
    }
}
