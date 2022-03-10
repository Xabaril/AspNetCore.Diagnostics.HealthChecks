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
