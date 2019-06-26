using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core.Discovery.Docker
{
    internal class DockerDiscoveryHostedService : IHostedService
    {
        private readonly DockerDiscoverySettings _discoveryOptions;
        private readonly ILogger<DockerDiscoveryHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDockerDiscoveryService _discoveryService;
        private readonly CancellationTokenSource _executingTaskCancellationTokenSource;

        private Task _executingTask;

        public DockerDiscoveryHostedService(
            IServiceProvider serviceProvider,
            IOptions<DockerDiscoverySettings> discoveryOptions,
            IDockerDiscoveryService discoveryService,
            ILogger<DockerDiscoveryHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _discoveryOptions = discoveryOptions?.Value ?? throw new ArgumentNullException(nameof(discoveryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _executingTaskCancellationTokenSource = new CancellationTokenSource();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_executingTaskCancellationTokenSource.Token);

            if (_executingTask.IsCompleted)
                return _executingTask;

            return Task.CompletedTask;
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _executingTaskCancellationTokenSource.Cancel();

            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var refreshTime = TimeSpan.FromSeconds(_discoveryOptions.RefreshTimeOnSeconds);
            var defaultPath = _discoveryOptions.HealthPath;
            const string defaultScheme = "http";

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Starting docker service discovery on endpoint {_discoveryOptions.Endpoint}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var registrationService = scope.ServiceProvider.GetRequiredService<IDiscoveryRegistryService>();

                    try
                    {
                        var containers = await _discoveryService.Discover(cancellationToken);

                        _logger.LogDebug("Identified {Count} containers", containers.Count);

                        foreach (var container in containers)
                        {
                            using (_logger.BeginScope(new Dictionary<string, object>
                            {
                                {"ContainerId", container.Id},
                                {"ContainerName", container.Name}
                            }))
                            {
                                UriBuilder builder = new UriBuilder();
                                builder.Scheme = container.Scheme ?? defaultScheme;
                                builder.Host = container.IP;

                                if (container.Port.HasValue)
                                    builder.Port = container.Port.Value;

                                if (container.Path != null)
                                    builder.Path = container.Path;
                                else
                                    builder.Path = defaultPath;

                                Uri uri = builder.Uri;

                                _logger.LogDebug("Container {ContainerId} has service address {Uri}", container.Id, uri);

                                await registrationService.RegisterService("docker", container.Name, uri, cancellationToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred on docker service discovery");
                    }
                }

                await Task.Delay(refreshTime, cancellationToken);
            }
        }
    }
}
