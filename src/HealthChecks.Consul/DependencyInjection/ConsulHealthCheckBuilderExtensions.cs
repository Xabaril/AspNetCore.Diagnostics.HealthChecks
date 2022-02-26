using System;
using System.Collections.Generic;
using System.Net.Http;
using HealthChecks.Consul;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="ConsulHealthCheck"/>.
    /// </summary>
    public static class ConsulHealthCheckBuilderExtensions
    {
        private const string NAME = "consul";

        /// <summary>
        /// Add a health check for Consul services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure Consul. </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'consul' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddConsul(
            this IHealthChecksBuilder builder,
            Action<ConsulOptions>? setup,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp => CreateHealthCheck(sp, setup, registrationName),
                failureStatus,
                tags,
                timeout));
        }

        private static ConsulHealthCheck CreateHealthCheck(IServiceProvider sp, Action<ConsulOptions>? setup, string name)
        {
            var options = new ConsulOptions();
            setup?.Invoke(options);

            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new ConsulHealthCheck(options, () => httpClientFactory.CreateClient(name));
        }
    }
}
