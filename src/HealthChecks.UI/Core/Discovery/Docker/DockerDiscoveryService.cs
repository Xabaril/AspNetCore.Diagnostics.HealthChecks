using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using HealthChecks.UI.Core.Discovery.Docker.Extensions;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.Core.Discovery.Docker
{
    internal class DockerDiscoveryService : IDockerDiscoveryService
    {
        private readonly ILogger<DockerDiscoveryService> _logger;
        private readonly string _labelPrefix;
        private readonly DockerClient _client;
        private readonly ContainersListParameters _listParameters = new ContainersListParameters();

        public DockerDiscoveryService(ILogger<DockerDiscoveryService> logger, Uri endpoint, string labelPrefix)
        {
            _logger = logger;
            _labelPrefix = labelPrefix;
            _client = new DockerClientConfiguration(endpoint)
                .CreateClient();
        }

        public async Task<IList<DockerDiscoveredContainer>> Discover(CancellationToken cancellationToken)
        {
            var containers = await _client.Containers.ListContainersAsync(_listParameters, cancellationToken);
            var res = new List<DockerDiscoveredContainer>();

            foreach (ContainerListResponse container in containers)
            {
                if (!container.TryGetLabel($"{_labelPrefix}Enabled", out bool isEnabled) || !isEnabled)
                    continue;

                DockerDiscoveredContainer result = new DockerDiscoveredContainer
                {
                    Id = container.ID
                };

                // Identify name
                if (container.TryGetLabel($"{_labelPrefix}Name", out string labelName))
                    result.Name = labelName;
                else if (container.Names.Any())
                    result.Name = container.Names.First();
                else
                    result.Name = container.ID;

                // Identify IP
                if (container.TryGetLabel($"{_labelPrefix}Network", out string networkName) &&
                    container.NetworkSettings.Networks.TryGetValue(networkName, out var networkSettings))
                    result.IP = networkSettings.IPAddress;
                else if (container.NetworkSettings.Networks.Any())
                    result.IP = container.NetworkSettings.Networks.First().Value.IPAddress;
                else
                {
                    _logger.LogWarning("Container {ContainerId} had no networks", container.ID);
                    continue;
                }

                // Identify URI components
                result.Scheme = container.GetLabel<string>($"{_labelPrefix}Scheme");
                result.Path = container.GetLabel<string>($"{_labelPrefix}Path");

                if (container.TryGetLabel($"{_labelPrefix}Port", out int portValue))
                    result.Port = portValue;
                else if (container.Ports.Any())
                    result.Port = container.Ports.First().PrivatePort;

                res.Add(result);
            }

            return res;
        }
    }
}