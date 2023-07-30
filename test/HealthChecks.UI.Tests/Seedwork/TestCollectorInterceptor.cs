using HealthChecks.UI.Core;
using HealthChecks.UI.Data;

namespace HealthChecks.UI.Tests;

internal class TestCollectorInterceptor : IHealthCheckCollectorInterceptor
{
    private readonly ManualResetEventSlim _resetEvent;

    public TestCollectorInterceptor(ManualResetEventSlim resetEvent)
    {
        _resetEvent = Guard.ThrowIfNull(resetEvent);
    }

    public ValueTask OnCollectExecuted(UIHealthReport report)
    {
        _resetEvent.Set();
        return default;
    }

    public ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck) => default;
}
