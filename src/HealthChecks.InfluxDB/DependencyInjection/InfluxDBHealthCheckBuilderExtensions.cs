using HealthChecks.InfluxDB;
using InfluxDB.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InfluxDBHealthCheckBuilderExtensions
    {
        const string NAME = "InfluxDB";

        /// <summary>
        /// Add a health check for InfluxDB services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="influxConnectionString">The InfluxDB connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string influxConnectionString, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(influxConnectionString));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for InfluxDB services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="InfluxDBConnectionString">The InfluxDB connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, Uri influxConnectionString, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(influxConnectionString));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for InfluxDB V1.0 services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDBV1(this IHealthChecksBuilder builder, string url, string username, char[] password, string database, string retentionPolicy, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(url,  username,  password,  database,  retentionPolicy));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for InfluxDB services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string url, string token, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(url, token));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for InfluxDB services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string url, string username, char[] password, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(url, username, password));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>

        /// Add a health check for InfluxDB services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, InfluxDBClientOptions options, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(options));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for InfluxDB services using <see cref="InfluxDBClient"/> from service provider.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddSingleton(sp =>
            {
                var influxdb_client = sp.GetService<InfluxDBClient>();
                if (influxdb_client != null)
                {
                    return new InfluxDBHealthCheck(influxdb_client);
                }
                else
                {
                    throw new ArgumentException($"An InfluxDBClient must be registered within the service provider");

                }
            });

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }
    }
}
