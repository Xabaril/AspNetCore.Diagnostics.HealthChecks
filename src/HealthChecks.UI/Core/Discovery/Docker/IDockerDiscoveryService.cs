using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Discovery.Docker
{
    internal interface IDockerDiscoveryService
    {
        Task<IList<DockerDiscoveredContainer>> Discover(CancellationToken cancellationToken);
    }
}