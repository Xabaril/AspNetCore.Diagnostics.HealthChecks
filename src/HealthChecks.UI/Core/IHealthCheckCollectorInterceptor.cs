using HealthChecks.UI.Data;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    public interface IHealthCheckCollectorInterceptor
    {
        ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck);
        ValueTask OnCollectExecuted(UIHealthReport report);
    }
}
