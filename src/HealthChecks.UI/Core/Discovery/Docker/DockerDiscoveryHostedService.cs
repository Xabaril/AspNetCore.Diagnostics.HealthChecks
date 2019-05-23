using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Core.Data;
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
        private readonly HttpClient _clusterServiceClient;

        private Task _executingTask;

        public DockerDiscoveryHostedService(
            IServiceProvider serviceProvider,
            IOptions<DockerDiscoverySettings> discoveryOptions,
            IHttpClientFactory httpClientFactory,
            IDockerDiscoveryService discoveryService,
            ILogger<DockerDiscoveryHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _discoveryOptions = discoveryOptions?.Value ?? throw new ArgumentNullException(nameof(discoveryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _clusterServiceClient = httpClientFactory.CreateClient(Keys.DOCKER_CLUSTER_SERVICE_HTTP_CLIENT_NAME);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(cancellationToken);

            if (_executingTask.IsCompleted)
                return _executingTask;

            return Task.CompletedTask;
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var refreshTime = TimeSpan.FromSeconds(_discoveryOptions.RefreshTimeOnSeconds);
            var defaultPath = $"/{_discoveryOptions.HealthPath}";
            const int defaultPort = 80;
            const string defaultScheme = "http";

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Starting docker service discovery on endpoint {_discoveryOptions.Endpoint}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

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
                                Uri serviceAddress = new Uri($"{container.Scheme ?? defaultScheme}://{container.IP}:{container.Port ?? defaultPort}{container.Path ?? defaultPath}");

                                _logger.LogDebug("Container {ContainerId} has service address {Uri}", container.Id, serviceAddress);

                                if (IsLivenessRegistered(db, serviceAddress))
                                {
                                    _logger.LogDebug("Skipping container, already registered");
                                    continue;
                                }

                                // Register it
                                try
                                {
                                    if (await IsValidHealthChecksStatusCode(serviceAddress))
                                    {
                                        await RegisterDiscoveredLiveness(db, container.Name, serviceAddress);

                                        _logger.LogInformation("Registered discovered liveness on {Address} with name {name}", serviceAddress, container.Name);
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Error discovering service {Name}", container.Name);
                                }
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

        bool IsLivenessRegistered(HealthChecksDb livenessDb, Uri uri)
        {
            string strUri = uri.ToString();
            return livenessDb.Configurations.Any(lc => lc.Uri == strUri);
        }

        async Task<bool> IsValidHealthChecksStatusCode(Uri uri)
        {
            using (var response = await _clusterServiceClient.GetAsync(uri))
                return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable;
        }
        Task<int> RegisterDiscoveredLiveness(HealthChecksDb db, string name, Uri uri)
        {
            db.Configurations.Add(new HealthCheckConfiguration
            {
                Name = name,
                Uri = uri.ToString(),
                DiscoveryService = "docker"
            });

            return db.SaveChangesAsync();
        }
    }
}
