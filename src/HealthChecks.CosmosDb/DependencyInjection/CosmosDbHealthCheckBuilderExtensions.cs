using Azure.Data.Tables;
using HealthChecks.CosmosDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosDbHealthCheckBuilderExtensions
    {
        const string COSMOS_NAME = "cosmosdb";
        const string TABLE_NAME = "azuretable";

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">CosmosDb full connection string.</param>
        /// <param name="database">Database to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            string connectionString,
            string database = default,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp => new CosmosDbHealthCheck(connectionString, database),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb/ Azure Storage table.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The CosmosDB Table or Azure Storage Table connection string. Credentials are included on connectionstring.</param>
        /// <param name="tableName">Table name to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            string connectionString,
            string tableName,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? TABLE_NAME,
               sp => new TableServiceHealthCheck(connectionString, tableName),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb/ Azure Storage table.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="endpoint">The CosmosDB Table or Azure Storage Table uri endopoint.</param>
        /// <param name="credentials">The table shared key credentials to be used.</param>
        /// <param name="tableName">Table name to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            Uri endpoint,
            TableSharedKeyCredential credentials,
            string tableName,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? TABLE_NAME,
               sp => new TableServiceHealthCheck(endpoint, credentials, tableName),
               failureStatus,
               tags,
               timeout));
        }
    }
}
