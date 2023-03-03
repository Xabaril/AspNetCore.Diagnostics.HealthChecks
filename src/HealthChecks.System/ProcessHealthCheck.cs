using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System;

public class ProcessHealthCheck : IHealthCheck
{
    private readonly string _processName;
    private readonly Func<Process[], bool> _predicate;

    public ProcessHealthCheck(string processName, Func<Process[], bool> predicate)
    {
        _processName = Guard.ThrowIfNull(processName);
        _predicate = Guard.ThrowIfNull(predicate);
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var processes = Process.GetProcessesByName(_processName);

            if (_predicate(processes))
            {
                return HealthCheckResultTask.Healthy;
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, exception: ex));
        }

        return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus));
    }
}
