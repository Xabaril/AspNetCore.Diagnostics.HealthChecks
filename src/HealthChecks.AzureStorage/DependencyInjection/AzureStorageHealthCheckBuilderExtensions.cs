using HealthChecks.AzureStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureStorageHealthCheckBuilderExtensions
    {
        const string AZURESTORAGE_NAME = "azureblob";
        const string AZURETABLE_NAME = "azuretable";
        const string AZUREQUEUE_NAME = "azurequeue";

        /// <summary>
        /// Add a health check for Azure Blob Storage.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Azure Storage connection string to be used. </param>
        /// <param name="containerName">The Azure Storage container name to check if exist. Optional, If <c>null</c> then container name check is not executed. </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureblob' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureBlobStorage(this IHealthChecksBuilder builder, string connectionString, string containerName = default, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? AZURESTORAGE_NAME,
               sp => new AzureBlobStorageHealthCheck(connectionString, containerName),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure Table Storage.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Azure Storage connection string to be used. </param>
        /// <param name="tableName">The Azure Storage table name to check if exist. Optional.If <c>null</c> then table name check is not executed. </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretable' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTableStorage(this IHealthChecksBuilder builder, string connectionString, string tableName = default, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? AZURETABLE_NAME,
               sp => new AzureTableStorageHealthCheck(connectionString, tableName),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure Queue Storage.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Azure Storage connection string to be used. </param>
        /// <param name="queueName">The Azure Storage queue name to check if exist. Optional.If <c>null</c> then queue name check is not executed. </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureQueueStorage(this IHealthChecksBuilder builder, string connectionString, string queueName = default, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? AZUREQUEUE_NAME,
               sp => new AzureQueueStorageHealthCheck(connectionString, queueName),
               failureStatus,
               tags,
               timeout));
        }
    }
}
