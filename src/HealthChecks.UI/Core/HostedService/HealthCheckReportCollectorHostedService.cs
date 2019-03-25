using HealthChecks.UI.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.HostedService
{
    internal class HealthCheckCollectorHostedService
        : IHostedService
    {
        private readonly ILogger<HealthCheckCollectorHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Settings _settings;

        private Task _executingTask;
        private CancellationTokenSource _cancellationTokenSource;

        public HealthCheckCollectorHostedService(IServiceProvider provider, IOptions<Settings> settings, ILogger<HealthCheckCollectorHostedService> logger)
        {
            _serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(provider));
            _settings = settings.Value ?? new Settings();
            _cancellationTokenSource = new CancellationTokenSource();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_cancellationTokenSource.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Executing HealthCheck collector HostedService.");

                using (var scope = scopeFactory.CreateScope())
                {
                    try
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<IHealthCheckReportCollector>();
                        await runner.Collect(cancellationToken);

                        _logger.LogDebug("HealthCheck collector HostedService executed succesfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"HealthCheck collector HostedService throw a error: {ex.Message}");
                    }
                }

                await Task.Delay(_settings.EvaluationTimeInSeconds * 1000, cancellationToken);
            }
        }
    }
}
