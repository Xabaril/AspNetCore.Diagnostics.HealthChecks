using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var errorList = new List<string>();
                var hangfireMonitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();

                // Check for maximum failed jobs
                if (_hangfireOptions.MaximumFailed.HasValue)
                {
                    var failedJobsCount = hangfireMonitoringApi.FailedCount();
                    if (failedJobsCount > _hangfireOptions.MaximumFailed)
                        errorList.Add($"#{failedJobsCount} failed jobs.");
                }

                // Check for minimum servers
                if (_hangfireOptions.MinimumServers.HasValue)
                {
                    var serversCount = hangfireMonitoringApi.Servers().Count;
                    if (serversCount < _hangfireOptions.MinimumServers)
                        errorList.Add($"{serversCount} server registered. Expected minimum {_hangfireOptions.MinimumServers}");                    
                }

                // If any error, set healt check status
                if (errorList.Any())
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: string.Join(" + ", errorList)));

                // Hangfire is healthy
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
