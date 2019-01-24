using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Hangfire
{
    public class HangfireHealthCheck
        : IHealthCheck
    {
        private readonly HangfireOptions _hangfireOptions = new HangfireOptions();
        public HangfireHealthCheck(HangfireOptions hangfireOptions)
        {
            _hangfireOptions = hangfireOptions;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var hangfireMonitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();

                var failedJobsCount = hangfireMonitoringApi.FailedCount();
                if (failedJobsCount > _hangfireOptions.MaximumFailed)
                {
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: $"#{failedJobsCount} failed jobs."));
                }
                    
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
