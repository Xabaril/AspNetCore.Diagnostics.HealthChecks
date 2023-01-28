using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Hangfire
{
    public class HangfireHealthCheck : IHealthCheck
    {
        private readonly HangfireOptions _hangfireOptions;

        public HangfireHealthCheck(HangfireOptions hangfireOptions)
        {
            _hangfireOptions = Guard.ThrowIfNull(hangfireOptions);
        }

        /// <inheritdoc />
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string>? errorList = null;
                var hangfireMonitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();

                if (_hangfireOptions.MaximumJobsFailed.HasValue)
                {
                    long failedJobsCount = hangfireMonitoringApi.FailedCount();
                    if (failedJobsCount >= _hangfireOptions.MaximumJobsFailed)
                    {
                        (errorList ??= new()).Add($"Hangfire has #{failedJobsCount} failed jobs and the maximum available is {_hangfireOptions.MaximumJobsFailed}.");
                    }
                }

                if (_hangfireOptions.MinimumAvailableServers.HasValue)
                {
                    int serversCount = hangfireMonitoringApi.Servers().Count;
                    if (serversCount < _hangfireOptions.MinimumAvailableServers)
                    {
                        (errorList ??= new()).Add($"{serversCount} server registered. Expected minimum {_hangfireOptions.MinimumAvailableServers}.");
                    }
                }

                if (errorList?.Count > 0)
                {
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: string.Join(" + ", errorList)));
                }

                return HealthCheckResultTask.Healthy;
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
