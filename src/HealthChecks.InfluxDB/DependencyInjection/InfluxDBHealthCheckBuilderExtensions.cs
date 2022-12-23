using HealthChecks.InfluxDB;
using InfluxDB.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InfluxDBHealthCheckBuilderExtensions
    {
        private const string NAME = "InfluxDB";

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
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string influxConnectionString, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
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
        /// <param name="influxConnectionString">The InfluxDB connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, Uri influxConnectionString, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
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
        /// Add a health check for InfluxDB V1.0 services using url.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="retentionPolicy"></param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDBV1(this IHealthChecksBuilder builder, string url, string username, string password, string database, string retentionPolicy, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new InfluxDBHealthCheck(url, username, password, database, retentionPolicy));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for InfluxDB services using url with token.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string url, string token, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
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
        /// Add a health check for InfluxDB services using url.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string url, string username, string password, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
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
        /// Add a health check for InfluxDB services using <see cref="InfluxDBClientOptions"/> .
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="options"></param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'InfluxDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, InfluxDBClientOptions options, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
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
        public static IHealthChecksBuilder AddInfluxDB(this IHealthChecksBuilder builder, string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddSingleton(sp => new InfluxDBHealthCheck(sp.GetRequiredService<InfluxDBClient>()));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<InfluxDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }
    }
}
