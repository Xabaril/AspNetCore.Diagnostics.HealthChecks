using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;

namespace HealthChecks.UI.Tests
{
    internal class TestCollectorInterceptor : IHealthCheckCollectorInterceptor
    {
        private readonly ManualResetEventSlim _resetEvent;

        public TestCollectorInterceptor(ManualResetEventSlim resetEvent)
        {
            _resetEvent = resetEvent ?? throw new ArgumentNullException(nameof(resetEvent));
        }

        public ValueTask OnCollectExecuted(UIHealthReport report)
        {
            _resetEvent.Set();
            return default;
        }

        public ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck) => default;
    }
}
