using HealthChecks.Consul;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConsulHealthCheckBuilderExtensions
    {
        private const string NAME = "consul";

        /// <summary>
        /// Add a health check for Consul services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The function to configure <see cref="ConsulOptions"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'consul' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">The timeout after which the health check is considered failed. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddConsul(this IHealthChecksBuilder builder, Action<ConsulOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var options = new ConsulOptions();
            setup?.Invoke(options);

            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => CreateHealthCheck(sp, options, registrationName, timeout ?? Timeout.InfiniteTimeSpan),
                failureStatus,
                tags));
        }
        static ConsulHealthCheck CreateHealthCheck(IServiceProvider sp, ConsulOptions options, string name, TimeSpan timeout)
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new ConsulHealthCheck(options, () => httpClientFactory.CreateClient(name), timeout);
        }
    }
}