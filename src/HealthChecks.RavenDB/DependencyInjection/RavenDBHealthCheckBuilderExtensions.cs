using System;
using HealthChecks.RavenDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RavenDBHealthCheckBuilderExtensions
    {
        const string NAME = "ravendb";

        /// <summary>
        /// Add a health check for RavenDB.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">Connection string to RavenDB.</param>
        /// <param name="databaseName">The specified database to check.</param>
        /// <param name="name">
        /// The health check name. Optional. If <see langword="null"/> the type name 'ravendb' will be used for the name.
        /// </param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        [Obsolete("This method is deprecated.")]
        public static IHealthChecksBuilder AddRavenDB(
            this IHealthChecksBuilder builder,
            string connectionString,
            string databaseName = default,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return builder.AddRavenDB(
                _ =>
                {
                    _.Urls = new[] {connectionString};
                    _.Database = databaseName;
                },
                name,
                failureStatus,
                tags);
        }


        /// <summary>
        /// Add a health check for RavenDB.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the RavenDB setup.</param>
        /// <param name="name">
        /// The health check name. Optional. If <see langword="null"/> the type name 'ravendb' will be used for the name.
        /// </param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddRavenDB(
            this IHealthChecksBuilder builder,
            Action<RavenDBOptions> setup,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            var options = new RavenDBOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new RavenDBHealthCheck(options),
                failureStatus,
                tags));
        }
    }
}
