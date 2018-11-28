using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.S3.DependencyInjection
{
    public static class S3HealthCheckBuilderExtensions
    {
        private const string Name = "aws s3";

        /// <summary>
        /// Add a health check for AWS S3.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the DynamoDb connection parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'dynamodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddS3(this IHealthChecksBuilder builder, Action<S3BucketOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            var options = new S3BucketOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? Name,
                sp => new S3HealthCheck(options),
                failureStatus,
                tags));
        }
    }
}
