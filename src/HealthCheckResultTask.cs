using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// This class is available to all health checks projects.
/// </summary>
internal static class HealthCheckResultTask
{
    /// <summary>
    /// Memory optimization that allows to not allocate new Task on green path in non-async health checks.
    /// </summary>
    public static readonly Task<HealthCheckResult> Healthy = Task.FromResult(HealthCheckResult.Healthy());
}


internal class HealthCheckErrorList : List<string>
{
    public Task<HealthCheckResult> GetHealthStateAsync(HealthCheckContext context)
    {
        if (Count > 0)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: string.Join("; ", this)));
        }

        return HealthCheckResultTask.Healthy;
    }

    public HealthCheckResult GetHealthState(HealthCheckContext context)
    {
        if (Count > 0)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: string.Join("; ", this));
        }

        return HealthCheckResult.Healthy();
    }
}
