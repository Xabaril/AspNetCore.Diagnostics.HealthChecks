using HealthChecks.UI.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    public interface IHealthCheckCollectorInterceptor
    {
        ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck);
        ValueTask OnCollectExecuted(UIHealthReport report);
    }
}
