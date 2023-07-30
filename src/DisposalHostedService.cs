using Microsoft.Extensions.Hosting;

internal sealed class DisposalHostedService : IHostedService
{
    private readonly IDisposable _disposable;

    public DisposalHostedService(IDisposable disposable)
    {
        _disposable = disposable;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected", Justification = "by design")]
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _disposable.Dispose();
        return Task.CompletedTask;
    }
}
