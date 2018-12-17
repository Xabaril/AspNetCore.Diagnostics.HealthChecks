using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class MaximumValueHealthCheck<T>
        : IHealthCheck
        where T : IComparable<T>
    {
        private readonly T maximumValue;
        private readonly Func<T> currentValueFunc;
        public MaximumValueHealthCheck(T maximumValue, Func<T> currentValueFunc)
        {
            this.maximumValue = maximumValue;
            this.currentValueFunc = currentValueFunc;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var currentValue = currentValueFunc();

            if (currentValue.CompareTo(maximumValue) <= 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(
                new HealthCheckResult(context.Registration.FailureStatus, description: $"Maximum={maximumValue}, Current={currentValue}"));
        }
    }
}
