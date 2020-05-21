using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalTests.Base
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
            return new ValueTask();
        }

        public ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck)
        {
            return new ValueTask();
        }
    }
}