using HealthChecks.AzureIoTHub;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureIoTHubHealthChecksBuilderExtensions
    {
        private const string HEALTH_QUERY = "SELECT deviceId FROM devices";
        const string IOTHUB_NAME = "azureiothub";

        /// <summary>
        /// Add a health check for Azure IoT Hub. Default behaviour is using Managed Service Identity, to use Client Secrets call UseClientSecrets in setup action
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">Azure IoT Hub connection string to use.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select devices with some deviceId is used.</param>    
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureiothub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureIoTHub(this IHealthChecksBuilder builder,
            string connectionString,
            string healthQuery = default,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return builder.Add(new HealthCheckRegistration(
               name ?? IOTHUB_NAME,
               sp => new AzureIoTHubHealthCheck(connectionString, healthQuery ?? HEALTH_QUERY),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure IoT Hub. Default behaviour is using Managed Service Identity, to use Client Secrets call UseClientSecrets in setup action
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionStringFactory">A factory to build the Azure IoT Hub connection string to use.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select devices with some deviceId is used.</param>    
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureiothub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureIoTHub(this IHealthChecksBuilder builder,
            Func<IServiceProvider, string> connectionStringFactory,
            string healthQuery = default,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            if (connectionStringFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionStringFactory));
            }
            
            return builder.Add(new HealthCheckRegistration(
               name ?? IOTHUB_NAME,
               sp => new AzureIoTHubHealthCheck(connectionStringFactory(sp), healthQuery ?? HEALTH_QUERY),
               failureStatus,
               tags,
               timeout));
        }
    }
}
