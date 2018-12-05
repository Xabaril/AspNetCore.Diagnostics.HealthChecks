using Google.Cloud.Datastore.V1;
using HealthChecks.Gcp.Datastore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatastoreHealthCheckBuilderExtensions
    {
        private const string Name = "google cloud datastore";

        /// <summary>
        /// Add a health check for Google Cloud Datastore.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="timeOut">TimeOut to abort connection in milliseconds.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'google cloud datastore' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddGoogleCloudDatastore(this IHealthChecksBuilder builder, int timeOut, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? Name,
                sp => new DatastoreHealthCheck(sp.GetService<DatastoreDb>(), timeOut),
                failureStatus,
                tags));
        }
    }
}
