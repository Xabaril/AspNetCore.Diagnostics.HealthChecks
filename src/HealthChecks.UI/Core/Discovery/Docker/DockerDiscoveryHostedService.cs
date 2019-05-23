using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Discovery.Docker.Extensions;
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
        private readonly HttpClient _clusterServiceClient;
        private readonly DockerClient _client;

        private Task _executingTask;

        public DockerDiscoveryHostedService(
            IServiceProvider serviceProvider,
            IOptions<DockerDiscoverySettings> discoveryOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<DockerDiscoveryHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _discoveryOptions = discoveryOptions?.Value ?? throw new ArgumentNullException(nameof(discoveryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _clusterServiceClient = httpClientFactory.CreateClient(Keys.DOCKER_CLUSTER_SERVICE_HTTP_CLIENT_NAME);

            _client = new DockerClientConfiguration(new Uri(_discoveryOptions.Endpoint))
                .CreateClient();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(cancellationToken);

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
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var refreshTime = TimeSpan.FromSeconds(_discoveryOptions.RefreshTimeOnSeconds);
            var labelPrefix = $"{_discoveryOptions.ServicesLabelPrefix}.";
            var listParameters = new ContainersListParameters();

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Starting docker service discovery on endpoint {_client.Configuration.EndpointBaseUri}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

                    try
                    {
                        var containers = await _client.Containers.ListContainersAsync(listParameters);

                        _logger.LogDebug("Identified {Count} containers", containers.Count);

                        foreach (ContainerListResponse container in containers)
                        {
                            if (!container.TryGetLabel($"{labelPrefix}Enabled", out bool isEnabled) || !isEnabled)
                                continue;

                            using (_logger.BeginScope(new Dictionary<string, object>
                            {
                                {"ContainerId", container.ID},
                                {"ContainerNames", container.Names}
                            }))
                            {
                                // Identify name
                                string name;
                                if (container.TryGetLabel($"{labelPrefix}Name", out string labelName))
                                    name = labelName;
                                else if (container.Names.Any())
                                    name = container.Names.First();
                                else
                                    name = container.ID;

                                // Create URI
                                string ip;
                                if (container.TryGetLabel($"{labelPrefix}Network", out string networkName) &&
                                    container.NetworkSettings.Networks.TryGetValue(networkName,
                                        out var networkSettings))
                                    ip = networkSettings.IPAddress;
                                else if (container.NetworkSettings.Networks.Any())
                                    ip = container.NetworkSettings.Networks.First().Value.IPAddress;
                                else
                                {
                                    _logger.LogWarning("Container {ContainerId} had no networks", container.ID);
                                    continue;
                                }

                                string scheme = container.GetLabel($"{labelPrefix}Scheme", "http");
                                string path = container.GetLabel($"{labelPrefix}Path", $"/{_discoveryOptions.HealthPath}");

                                int port;
                                if (container.TryGetLabel($"{labelPrefix}Port", out int portValue))
                                    port = portValue;
                                else if (container.Ports.Any())
                                    port = container.Ports.First().PrivatePort;
                                else
                                    port = 80;

                                Uri serviceAddress = new Uri($"{scheme}://{ip}:{port}{path}");

                                _logger.LogDebug("Container {ContainerId} has service address {Uri}", container.ID, serviceAddress);

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
                                        await RegisterDiscoveredLiveness(db, name, serviceAddress);

                                        _logger.LogInformation("Registered discovered liveness on {Address} with name {name}", serviceAddress, name);
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Error discovering service {Name}", name);
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
