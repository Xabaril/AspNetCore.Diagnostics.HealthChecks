using Azure.Core;
using HealthChecks.AzureDigitalTwin;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Rest;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureDigitalTwinHealthCheckBuilderExtensions
    {
        const string AZUREDIGITALTWIN_NAME = "azuredigitaltwin";
        const string AZUREDIGITALTWINMODELS_NAME = "azuredigitaltwinmodels";
        const string AZUREDIGITALTWININSTANCE_NAME = "azuredigitaltwininstance";

        /// <summary>
        /// Add a health check for specified Azure Digital Twin liveness.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="clientId">The azure digital twin client id.</param>
        /// <param name="clientSecret">The azure digital twin client secret.</param>
        /// <param name="tenantId">The azure digital twin tenant id.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureDigitalTwin(this IHealthChecksBuilder builder, string clientId, string clientSecret, string tenantId, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREDIGITALTWIN_NAME,
                sp => new AzureDigitalTwinHealthCheck(clientId, clientSecret, tenantId),
                failureStatus,
                tags,
                timeout));
        }
        
        /// <summary>
        /// Add a health check for specified Azure Digital Twin liveness.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="credentials">The azure digital twin credentials.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureDigitalTwin(this IHealthChecksBuilder builder, ServiceClientCredentials credentials, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREDIGITALTWIN_NAME,
                sp => new AzureDigitalTwinHealthCheck(credentials),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for specified Azure Digital Twin existing models.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="clientId">The azure digital twin client id.</param>
        /// <param name="clientSecret">The azure digital twin client secret.</param>
        /// <param name="tenantId">The azure digital twin tenant id.</param>
        /// <param name="hostName">The azure digital host uri.</param>
        /// <param name="models">The azure digital twin model collection expected.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureDigitalTwinModels(this IHealthChecksBuilder builder, string clientId, string clientSecret, string tenantId, string hostName, string[] models, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREDIGITALTWINMODELS_NAME,
                sp => new AzureDigitalTwinModelsHealthCheck(clientId, clientSecret, tenantId, hostName, models),
                failureStatus,
                tags,
                timeout));
        }
        
        /// <summary>
        /// Add a health check for specified Azure Digital Twin existing models.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="credentials">The azure digital twin credentials.</param>
        /// <param name="hostName">The azure digital host uri.</param>
        /// <param name="models">The azure digital twin model collection expected.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureDigitalTwinModels(this IHealthChecksBuilder builder, TokenCredential credentials, string hostName, string[] models, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREDIGITALTWINMODELS_NAME,
                sp => new AzureDigitalTwinModelsHealthCheck(credentials, hostName, models),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for specified Azure Digital Twin instance.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="clientId">The azure digital twin client id.</param>
        /// <param name="clientSecret">The azure digital twin client secret.</param>
        /// <param name="tenantId">The azure digital twin tenant id.</param>
        /// <param name="hostName">The azure digital host uri.</param>
        /// <param name="instanceName">The azure digital twin instance id.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureDigitalTwinInstance(this IHealthChecksBuilder builder, string clientId, string clientSecret, string tenantId, string hostName, string instanceName, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREDIGITALTWININSTANCE_NAME,
                sp => new AzureDigitalTwinInstanceHealthCheck(clientId, clientSecret, tenantId, hostName, instanceName),
                failureStatus,
                tags,
                timeout));
        }
        
        /// <summary>
        /// Add a health check for specified Azure Digital Twin instance.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="credentials">The azure digital twin credentials.</param>
        /// <param name="hostName">The azure digital host uri.</param>
        /// <param name="instanceName">The azure digital twin instance id.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="requiresSession">An optional boolean flag that indicates whether session is enabled on the queue or not. Defaults to false.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureDigitalTwinInstance(this IHealthChecksBuilder builder, TokenCredential credentials, string hostName, string instanceName, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREDIGITALTWININSTANCE_NAME,
                sp => new AzureDigitalTwinInstanceHealthCheck(credentials, hostName, instanceName),
                failureStatus,
                tags,
                timeout));
        }
    }
}
