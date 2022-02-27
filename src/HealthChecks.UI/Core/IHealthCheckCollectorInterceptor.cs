using HealthChecks.UI.Core.Data;

namespace HealthChecks.UI.Core
{
    public interface IHealthCheckCollectorInterceptor
    {
        ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck);
        ValueTask OnCollectExecuted(UIHealthReport report);
    }
}
