using HealthChecks.Aws.S3;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class S3HealthCheckBuilderExtensions
    {
        private const string Name = "aws s3";

        /// <summary>
        /// Add a health check for AWS S3.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the S3 Configuration e.g. bucket, region etc. </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'aws s3' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddS3(this IHealthChecksBuilder builder, Action<S3BucketOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default,TimeSpan? timeout = default)
        {
            var options = new S3BucketOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? Name,
                sp => new S3HealthCheck(options),
                failureStatus,
                tags,
                timeout));
        }
    }
}
