using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kubernetes
{
    public class KubernetesHealthCheck : IHealthCheck
    {
        private readonly KubernetesHealthCheckBuilder _builder;
        private readonly KubernetesChecksExecutor _kubernetesChecksExecutor;
        public KubernetesHealthCheck(KubernetesHealthCheckBuilder builder,
            KubernetesChecksExecutor kubernetesChecksExecutor)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _kubernetesChecksExecutor = kubernetesChecksExecutor ??
                                        throw new ArgumentNullException(nameof(kubernetesChecksExecutor));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            List<Task<(bool result, string name)>> checks = new List<Task<(bool, string)>>();

            try
            {
                foreach (var item in _builder.Options.Registrations)
                {
                    checks.Add(_kubernetesChecksExecutor.CheckAsync(item, cancellationToken));
                }

                var results = await Task.WhenAll(checks).PreserveMultipleExceptions();

                if (results.Any(r => !r.result))
                {
                    var resultsNotMeetingConditions = results.Where(r => !r.result).Select(r => r.name);
                    return new HealthCheckResult(context.Registration.FailureStatus,
                        $"Kubernetes resources with failed conditions: {string.Join(",", resultsNotMeetingConditions)}");
                }

                return HealthCheckResult.Healthy();
            }
            catch (AggregateException ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, string.Join(",", ex.InnerExceptions.Select(s => s.Message)));
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, ex.Message);
            }
        }
    }
}