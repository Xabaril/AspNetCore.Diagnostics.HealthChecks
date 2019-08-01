using System;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureServiceBusHealthCheckBuilderExtensions
    {
        const string AZUREEVENTHUB_NAME = "azureeventhub";
        const string AZUREQUEUE_NAME = "azurequeue";
        const string AZURETOPIC_NAME = "azuretopic";

        /// <summary>
        /// Add a health check for specified Azure Event Hub.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The azure event hub connection string.</param>
        /// <param name="eventHubName">The azure event hub name.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureEventHub(this IHealthChecksBuilder builder, string connectionString, string eventHubName, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREEVENTHUB_NAME,
                sp => new AzureEventHubHealthCheck(connectionString, eventHubName),
                failureStatus,
                tags));
        }
        /// <summary>
        /// Add a health check for specified Azure Service Bus Queue.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The azure service bus connection string to be used.</param>
        /// <param name="queueName">The name of the queue to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="partitionKeySelector">Partition key callback. Optional. If <c>null</c> no partition key will be used.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureServiceBusQueue(this IHealthChecksBuilder builder, string connectionString, string queueName, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, Func<AzureServiceBusQueueHealthCheck, string> partitionKeySelector = null)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZUREQUEUE_NAME,
                sp => new AzureServiceBusQueueHealthCheck(connectionString, queueName, partitionKeySelector),
                failureStatus,
                tags));
        }
        /// <summary>
        /// Add a health check for Azure Service Bus Topic.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
        /// <param name="topicName">The topic name of the topic to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="partitionKeySelector">Partition key callback. Optional. If <c>null</c> no partition key will be used.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureServiceBusTopic(this IHealthChecksBuilder builder, string connectionString, string topicName, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, Func<AzureServiceBusTopicHealthCheck, string> partitionKeySelector = null)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? AZURETOPIC_NAME,
                sp => new AzureServiceBusTopicHealthCheck(connectionString, topicName, partitionKeySelector),
                failureStatus,
                tags));
        }
    }
}
