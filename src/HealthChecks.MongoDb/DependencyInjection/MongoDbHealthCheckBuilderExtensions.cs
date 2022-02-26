using System;
using System.Collections.Generic;
using HealthChecks.MongoDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="MongoDbHealthCheck"/>.
    /// </summary>
    public static class MongoDbHealthCheckBuilderExtensions
    {
        private const string NAME = "mongodb";

        /// <summary>
        /// Add a health check for MongoDb database that list all databases on the system.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="mongodbConnectionString">The MongoDb connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddMongoDb(
            this IHealthChecksBuilder builder,
            string mongodbConnectionString,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new MongoDbHealthCheck(mongodbConnectionString),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for MongoDb database that list all collections from specified database on <paramref name="mongoDatabaseName"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="mongodbConnectionString">The MongoDb connection string to be used.</param>
        /// <param name="mongoDatabaseName">The Database name to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddMongoDb(
            this IHealthChecksBuilder builder,
            string mongodbConnectionString,
            string mongoDatabaseName,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new MongoDbHealthCheck(mongodbConnectionString, mongoDatabaseName),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for MongoDb that list all databases from specified <paramref name="mongoClientSettings"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="mongoClientSettings">The MongoClientSettings to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddMongoDb(
            this IHealthChecksBuilder builder,
            MongoClientSettings mongoClientSettings,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new MongoDbHealthCheck(mongoClientSettings),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for MongoDb database that list all collections from specified database on <paramref name="mongoDatabaseName"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="mongoClientSettings">The MongoClientSettings to be used.</param>
        /// <param name="mongoDatabaseName">The Database name to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddMongoDb(
            this IHealthChecksBuilder builder,
            MongoClientSettings mongoClientSettings,
            string mongoDatabaseName,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new MongoDbHealthCheck(mongoClientSettings, mongoDatabaseName),
                failureStatus,
                tags,
                timeout));
        }
    }
}
