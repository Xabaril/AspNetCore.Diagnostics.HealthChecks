using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using HealthChecks.Kubernetes;
using k8s;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KubernetesHealthCheckBuilderExtensions
    {
        const string NAME = "k8s";
        /// <summary>
        /// Add the Kubernetes Health Check
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">Action to configure Kubernetes cluster and registrations</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'k8s' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddKubernetes(this IHealthChecksBuilder builder, Action<KubernetesHealthCheckBuilder> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var kubernetesHealthCheckBuilder = new KubernetesHealthCheckBuilder();
            setup?.Invoke(kubernetesHealthCheckBuilder);

            if (kubernetesHealthCheckBuilder.Configuration == null)
            {
                throw new ArgumentNullException(nameof(kubernetesHealthCheckBuilder.Configuration));
            }

            var client = new Kubernetes(kubernetesHealthCheckBuilder.Configuration);
            var kubernetesChecksExecutor = new KubernetesChecksExecutor(client);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new KubernetesHealthCheck(kubernetesHealthCheckBuilder, kubernetesChecksExecutor),
                failureStatus,
                tags,
                timeout));
        }
    }
}
