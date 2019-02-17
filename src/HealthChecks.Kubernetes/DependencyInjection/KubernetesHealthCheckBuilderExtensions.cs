using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using HealthChecks.Kubernetes;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KubernetesHealthCheckBuilderExtensions
    {
        const string NAME = "k8s";

        /// <summary>
        /// Add a health check for Kafka cluster.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="config">The kafka connection configuration parameters to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddKubernetes(this IHealthChecksBuilder builder, Action<KubernetesHealthCheckBuilder> setup,  string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            var kubernetesHealthCheckBuilder = new KubernetesHealthCheckBuilder();
            setup?.Invoke(kubernetesHealthCheckBuilder);
            
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new KubernetesHealthCheck(kubernetesHealthCheckBuilder),
                failureStatus,
                tags));
        }
    }
}
