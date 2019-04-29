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
        private readonly HangfireOptions _hangfireOptions;
        public HangfireHealthCheck(HangfireOptions hangfireOptions)
        {
            _hangfireOptions = hangfireOptions ?? throw new ArgumentNullException(nameof(hangfireOptions));
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var errorList = new List<string>();
                var hangfireMonitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();

                if (_hangfireOptions.MaximumJobsFailed.HasValue)
                {
                    var failedJobsCount = hangfireMonitoringApi.FailedCount();
                    if (failedJobsCount >= _hangfireOptions.MaximumJobsFailed)
                    {
                        errorList.Add($"Hangfire has #{failedJobsCount} failed jobs and the maximum available is {_hangfireOptions.MaximumJobsFailed}.");
                    }
                }

                if (_hangfireOptions.MinimumAvailableServers.HasValue)
                {
                    var serversCount = hangfireMonitoringApi.Servers().Count;
                    if (serversCount < _hangfireOptions.MinimumAvailableServers)
                    {
                        errorList.Add($"{serversCount} server registered. Expected minimum {_hangfireOptions.MinimumAvailableServers}.");
                    }
                }

                if (errorList.Any())
                {
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: string.Join(" + ", errorList)));
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
