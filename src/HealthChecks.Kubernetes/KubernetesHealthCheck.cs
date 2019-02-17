using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kubernetes
{
    public class KubernetesHealthCheck : IHealthCheck
    {
        private readonly KubernetesHealthCheckBuilder _builder;
        private readonly KubernetesChecksExecutor _kubernetesChecksExecutor;

        public KubernetesHealthCheck(KubernetesHealthCheckBuilder builder, KubernetesChecksExecutor kubernetesChecksExecutor)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _kubernetesChecksExecutor = kubernetesChecksExecutor ?? throw new ArgumentNullException(nameof(kubernetesChecksExecutor));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            List<Task<(bool result, string name)>> checks = new List<Task<(bool, string)>>();
            try
            {

            foreach (var item in _builder.Options.Registrations)
            {
                checks.Add(_kubernetesChecksExecutor.CheckAsync(item));
            }

            var results = await Task.WhenAll(checks);
            
            if (checks.Any(t => t.IsFaulted))
            {
                var errors = checks.Where(t => t.IsFaulted).Select(t => t.Exception.Message);
                return new HealthCheckResult(context.Registration.FailureStatus,
                    $"Error executing kubernetes checks: {string.Join(",", errors)}");
            }

            if (results.Any(r => !r.result))
            {
                var resultWithNotMetConditions = results.Where(r => !r.result).Select(r => r.name);
                return new HealthCheckResult(context.Registration.FailureStatus, $"Kubernetes resources with failed conditions: {string.Join(",", resultWithNotMetConditions)}");
            }
        
            return HealthCheckResult.Healthy();
            
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}