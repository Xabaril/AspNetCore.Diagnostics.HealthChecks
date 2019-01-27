using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Hangfire
{
    public class HangfireServerHealthCheck
        : IHealthCheck
    {
        private readonly int _minimumServers;

        public HangfireServerHealthCheck(int minimumServers)
        {
            _minimumServers = minimumServers;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var hangfireMonitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();

                var serversCount = hangfireMonitoringApi.Servers().Count;
                if (serversCount < _minimumServers)
                {
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: $"{serversCount} server registered. Expected minimum {_minimumServers}"));
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
