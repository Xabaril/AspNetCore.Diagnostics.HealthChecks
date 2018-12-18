using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.HostedService
{
    interface IHealthCheckReportCollector : IDisposable
    {
        Task Collect(CancellationToken cancellationToken);
    }
}
