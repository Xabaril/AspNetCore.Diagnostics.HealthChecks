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
    class LivenessHostedService
        : IHostedService
    {
        private readonly ILogger<LivenessHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Settings _settings;

        private Task _executingTask;

        public LivenessHostedService(IServiceProvider provider,IOptions<Settings> settings, ILogger<LivenessHostedService> logger)
        {
            _serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(provider));
            _settings = settings.Value ?? new Settings();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteASync(cancellationToken);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task ExecuteASync(CancellationToken cancellationToken)
        {
            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Executing Background service.");

                using (var scope = scopeFactory.CreateScope())
                {
                    var runner = scope.ServiceProvider
                        .GetRequiredService<ILivenessRunner>();

                    try
                    {
                        await runner.Run(cancellationToken);

                        _logger.LogDebug("BackgroundService executed succesfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BackgroundService throw a error:", ex);
                    }  
                }

                await Task.Delay(_settings.EvaluationTimeOnSeconds * 1000);
            }
        }
    }
}
