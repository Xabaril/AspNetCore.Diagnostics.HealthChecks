using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.ApplicationStatus;

/// <summary>
/// Healthcheck that detects application graceful shutdown.
/// </summary>
public class ApplicationStatusHealthCheck : IHealthCheck, IDisposable
{
    private readonly IHostApplicationLifetime _lifetime;
    private CancellationTokenRegistration _ctRegistration = default;
    private bool IsApplicationRunning => _ctRegistration != default;

    public ApplicationStatusHealthCheck(IHostApplicationLifetime lifetime)
    {
        _lifetime = Guard.ThrowIfNull(lifetime);
        _ctRegistration = _lifetime.ApplicationStopping.Register(OnStopping);
    }

    /// <summary>
    /// Handler that will be triggered on application stopping event.
    /// </summary>
    private void OnStopping()
    {
        Dispose();
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(IsApplicationRunning ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }

    public void Dispose()
    {
        _ctRegistration.Dispose();
        _ctRegistration = default;
    }
}
