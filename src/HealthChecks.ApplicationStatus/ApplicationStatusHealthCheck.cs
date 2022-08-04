using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.ApplicationStatus;

public class ApplicationStatusHealthCheck : IHealthCheck, IDisposable
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly CancellationTokenRegistration _ctRegistration;
    private bool IsApplicationRunning { get; set; } = true;
    public ApplicationStatusHealthCheck(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(IHostApplicationLifetime));
        _ctRegistration = _lifetime.ApplicationStopping.Register(OnStopping);
    }

    private void OnStopping()
    {
        IsApplicationRunning = false;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(IsApplicationRunning ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }

    public void Dispose() => _ctRegistration.Dispose();
}
