using Azure.Core;
using Azure.Data.Tables;
using HealthChecks.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="CosmosDbHealthCheck"/> and <see cref="TableServiceHealthCheck"/>.
    /// </summary>
    public static class CosmosDbHealthCheckBuilderExtensions
    {
        private const string COSMOS_NAME = "cosmosdb";
        private const string TABLE_NAME = "azuretable";

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">CosmosDb full connection string.</param>
        /// <param name="database">Database to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            string connectionString,
            string? database = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp => new CosmosDbHealthCheck(connectionString, database, Enumerable.Empty<string>()),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="accountEndpoint">Uri to the CosmosDb account</param>
        /// <param name="tokenCredential">An instance of <see cref="TokenCredential"/> to be used for authentication</param>
        /// <param name="database">Database to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            string accountEndpoint,
            TokenCredential tokenCredential,
            string? database = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp => new CosmosDbHealthCheck(accountEndpoint, tokenCredential, database, Enumerable.Empty<string>()),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <remarks>
        /// A <see cref="CosmosClient"/> service must be registered in the DI container.
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            Action<CosmosDbHealthCheckOptions>? configureOptions = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp =>
               {
                   var options = new CosmosDbHealthCheckOptions();
                   configureOptions?.Invoke(options);
                   return new CosmosDbHealthCheck(sp.GetRequiredService<CosmosClient>(), options);
               },
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <remarks>
        /// A <see cref="CosmosClient"/> service must be registered in the DI container.
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            Action<IServiceProvider, CosmosDbHealthCheckOptions>? configureOptions = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp =>
               {
                   var options = new CosmosDbHealthCheckOptions();
                   configureOptions?.Invoke(sp, options);
                   return new CosmosDbHealthCheck(sp.GetRequiredService<CosmosClient>(), options);
               },
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="client">The <see cref="CosmosClient"/> instance that will communicate with the database.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            CosmosClient client,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp => new CosmosDbHealthCheck(client),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="client">The <see cref="CosmosClient"/> instance that will communicate with the database.</param>
        /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            CosmosClient client,
            Action<CosmosDbHealthCheckOptions> configureOptions,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp =>
               {
                   var options = new CosmosDbHealthCheckOptions();
                   configureOptions?.Invoke(options);
                   return new CosmosDbHealthCheck(client, options);
               },
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="client">The <see cref="CosmosClient"/> instance that will communicate with the database.</param>
        /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDb(
            this IHealthChecksBuilder builder,
            CosmosClient client,
            Action<IServiceProvider, CosmosDbHealthCheckOptions> configureOptions,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp =>
               {
                   var options = new CosmosDbHealthCheckOptions();
                   configureOptions?.Invoke(sp, options);
                   return new CosmosDbHealthCheck(client, options);
               },
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database and specified collections.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">CosmosDb full connection string.</param>
        /// <param name="database">Database to check for existence.</param>
        /// <param name="collections">Cosmos DB collections to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDbCollection(
            this IHealthChecksBuilder builder,
            string connectionString,
            string? database = default,
            IEnumerable<string>? collections = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp => new CosmosDbHealthCheck(connectionString, database, collections),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure CosmosDb database and specified collections using Managed identity.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="accountEndpoint">Uri to the CosmosDb account</param>
        /// <param name="tokenCredential">An instance of <see cref="TokenCredential"/> to be used for authentication</param>
        /// <param name="database">Database to check for existence.</param>
        /// <param name="collections">Cosmos DB collections to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddCosmosDbCollection(
            this IHealthChecksBuilder builder,
            string accountEndpoint,
            TokenCredential tokenCredential,
            string? database = default,
            IEnumerable<string>? collections = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? COSMOS_NAME,
               sp => new CosmosDbHealthCheck(accountEndpoint, tokenCredential, database, collections),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure Tables hosted in either Azure storage accounts or Azure Cosmos DB table API.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The CosmosDB Table or Azure Storage Table connection string. Credentials are included on connectionstring.</param>
        /// <param name="tableName">Table name to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            string connectionString,
            string tableName,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
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
        /// Add a health check for Azure Tables hosted in either Azure storage accounts or Azure Cosmos DB table API.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="endpoint">The CosmosDB Table or Azure Storage Table uri endpoint.</param>
        /// <param name="credentials">The table shared key credentials to be used.</param>
        /// <param name="tableName">Table name to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            Uri endpoint,
            TableSharedKeyCredential credentials,
            string? tableName,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? TABLE_NAME,
               sp => new TableServiceHealthCheck(endpoint, credentials, tableName),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure Tables hosted in either Azure storage accounts or Azure Cosmos DB table API.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="endpoint">The CosmosDB Table or Azure Storage Table uri endpoint.</param>
        /// <param name="tokenCredential">An instance of <see cref="TokenCredential"/> to be used for authentication</param>
        /// <param name="tableName">Table name to check for existence.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            Uri endpoint,
            TokenCredential tokenCredential,
            string tableName,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? TABLE_NAME,
               sp => new TableServiceHealthCheck(endpoint, tokenCredential, tableName),
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure Tables hosted in either Azure storage accounts or Azure Cosmos DB table API.
        /// </summary>
        /// <remarks>
        /// A <see cref="TableServiceClient"/> service must be registered in the DI container.
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            Action<TableServiceHealthCheckOptions>? configureOptions = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? TABLE_NAME,
               sp =>
               {
                   var options = new TableServiceHealthCheckOptions();
                   configureOptions?.Invoke(options);
                   return new TableServiceHealthCheck(sp.GetRequiredService<TableServiceClient>(), options);
               },
               failureStatus,
               tags,
               timeout));
        }

        /// <summary>
        /// Add a health check for Azure Tables hosted in either Azure storage accounts or Azure Cosmos DB table API.
        /// </summary>
        /// <remarks>
        /// A <see cref="TableServiceClient"/> service must be registered in the DI container.
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
        /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'cosmosdb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTable(
            this IHealthChecksBuilder builder,
            Action<IServiceProvider, TableServiceHealthCheckOptions>? configureOptions = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
               name ?? TABLE_NAME,
               sp =>
               {
                   var options = new TableServiceHealthCheckOptions();
                   configureOptions?.Invoke(sp, options);
                   return new TableServiceHealthCheck(sp.GetRequiredService<TableServiceClient>(), options);
               },
               failureStatus,
               tags,
               timeout));
        }
    }
}
