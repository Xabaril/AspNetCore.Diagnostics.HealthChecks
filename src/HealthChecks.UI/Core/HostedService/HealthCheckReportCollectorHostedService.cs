using HealthChecks.UI.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core.HostedService
{
    internal class HealthCheckCollectorHostedService : IHostedService
    {
        private readonly ILogger<HealthCheckCollectorHostedService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ServerAddressesService _serverAddressesService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Settings _settings;

        private Task? _executingTask;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public HealthCheckCollectorHostedService
            (IServiceProvider provider,
            IOptions<Settings> settings,
            ServerAddressesService serverAddressesService,
            ILogger<HealthCheckCollectorHostedService> logger,
            IHostApplicationLifetime lifetime)
        {
            _serviceProvider = Guard.ThrowIfNull(provider);
            _serverAddressesService = Guard.ThrowIfNull(serverAddressesService);
            _logger = Guard.ThrowIfNull(logger);
            _lifetime = Guard.ThrowIfNull(lifetime);
            _settings = settings.Value ?? new Settings();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_cancellationTokenSource.Token);

            return _executingTask.IsCompleted
                ? _executingTask
                : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            if (_executingTask != null)
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    await CollectAsync(cancellationToken);
                }
                catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // We are halting, task cancellation is expected.
                }
            });

            return Task.CompletedTask;
        }

        private async Task CollectAsync(CancellationToken cancellationToken)
        {
            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            while (!cancellationToken.IsCancellationRequested)
            {
                // Collect should not be triggered until IServerAddressFeature reports the listening endpoints

                _logger.LogDebug("Executing HealthCheck collector HostedService.");

                using (var scope = scopeFactory.CreateScope())
                {
                    try
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<IHealthCheckReportCollector>();
                        await runner.Collect(cancellationToken);

                        _logger.LogDebug("HealthCheck collector HostedService executed successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "HealthCheck collector HostedService threw an error: {Error}", ex.Message);
                    }
                }

                await Task.Delay(_settings.EvaluationTimeInSeconds * 1000, cancellationToken);
            }
        }
    }
}
