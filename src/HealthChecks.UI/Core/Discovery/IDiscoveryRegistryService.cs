using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Discovery
{
    internal interface IDiscoveryRegistryService
    {
        Task RegisterService(string service, string name, Uri uri, CancellationToken cancellationToken = default);
    }
}