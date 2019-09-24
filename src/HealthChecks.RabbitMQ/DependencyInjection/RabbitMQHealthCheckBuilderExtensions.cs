using HealthChecks.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQHealthCheckBuilderExtensions
    {
        const string NAME = "rabbitmq";

        /// <summary>
        /// Add a health check for RabbitMQ services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="rabbitMQConnectionString">The RabbitMQ connection string to be used.</param>
        /// <param name="sslOption">The RabbitMQ ssl options. Optional. If <c>null</c>, the ssl option will counted as disabled and not used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, string rabbitMQConnectionString, SslOption sslOption = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new RabbitMQHealthCheck(rabbitMQConnectionString, sslOption),
                failureStatus,
                tags,
                timeout));
        }
    }
}
