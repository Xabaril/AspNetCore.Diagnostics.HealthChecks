using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class ProcessAllocatedMemoryHealthCheck : IHealthCheck
    {
        private readonly int _maximumBytesAllocated;

        public ProcessAllocatedMemoryHealthCheck(int maximumBytesAllocated)
        {
            _maximumBytesAllocated = maximumBytesAllocated;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var totalMemoryBytes = GC.GetTotalMemory(false) / 1024 / 1024;

            if (totalMemoryBytes >= _maximumBytesAllocated)
            {
                return Task.FromResult(
                    new HealthCheckResult(
                        context.Registration.FailureStatus,
                        description: $"Allocated megabytes in memory beyond threshold: {totalMemoryBytes} mb / {_maximumBytesAllocated} mb"));
            }

            return Task.FromResult(HealthCheckResult.Healthy(description: $"Allocated megabytes in memory: {totalMemoryBytes} mb"));

        }
    }
}
