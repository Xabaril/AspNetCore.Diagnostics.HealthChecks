using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.HostedService
{
    internal interface IHealthCheckReportCollector
    {
        Task Collect(CancellationToken cancellationToken);
    }
}
