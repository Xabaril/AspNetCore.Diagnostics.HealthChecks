using HealthChecks.IdSvr;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdSvrHealthCheckBuilderExtensions
    {
        const string NAME = "idsvr";

        /// <summary>
        /// Add a health check for Identity Server.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="idSvrUri">The uri of the Identity Server to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'idsvr' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddIdentityServer(this IHealthChecksBuilder builder, Uri idSvrUri, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new IdSvrHealthCheck(idSvrUri),
                failureStatus,
                tags));
        }
    }
}
