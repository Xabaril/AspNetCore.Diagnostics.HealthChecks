using Microsoft.Extensions.Hosting;

namespace HealthChecks.ApplicationStatus.Tests;

public class TestHostApplicationLifeTime : IHostApplicationLifetime
{
    private readonly CancellationTokenSource _startedSource = new();
    private readonly CancellationTokenSource _stoppingSource = new();
    private readonly CancellationTokenSource _stoppedSource = new();
    public CancellationToken ApplicationStarted => _startedSource.Token;

    public CancellationToken ApplicationStopping => _stoppingSource.Token;

    public CancellationToken ApplicationStopped => _stoppedSource.Token;

    public void StopApplication()
    {
        ExecuteHandlers(_stoppingSource);
    }

    private void ExecuteHandlers(CancellationTokenSource cancel)
    {
        // Noop if this is already cancelled
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        // Run the cancellation token callbacks
        cancel.Cancel(throwOnFirstException: false);
    }
}
