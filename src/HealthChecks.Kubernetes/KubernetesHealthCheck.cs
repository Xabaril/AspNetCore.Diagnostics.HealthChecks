using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Kubernetes.KubernetesResourceChecks;
using k8s.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kubernetes
{
    public class KubernetesHealthCheck : IHealthCheck
    {
        private readonly KubernetesHealthCheckBuilder _builder;

        public KubernetesHealthCheck(KubernetesHealthCheckBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}