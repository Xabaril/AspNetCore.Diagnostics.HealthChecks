using HealthChecks.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisHealthCheckBuilderExtensions
    {
        const string NAME = "redis";

        /// <summary>
        /// Add a health check for Redis services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="redisConnectionString">The Redis connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddRedis(this IHealthChecksBuilder builder, string redisConnectionString, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.AddRedis(_ => redisConnectionString, name, failureStatus, tags, timeout);
        }

        /// <summary>
        /// Add a health check for Redis services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// /// <param name="connectionStringFactory">A factory to build the Redis connection string to use.</param>
        /// <param name="redisConnectionString">The Redis connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddRedis(this IHealthChecksBuilder builder,
            Func<IServiceProvider, string> connectionStringFactory,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            if (connectionStringFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionStringFactory));
            }

            return builder.Add(new HealthCheckRegistration(
               name ?? NAME,
               sp => new RedisHealthCheck(connectionStringFactory(sp)),
               failureStatus,
               tags,
               timeout));
        }
    }
}
