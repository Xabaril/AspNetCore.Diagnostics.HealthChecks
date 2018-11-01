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
        private readonly T maximunValue;
        private readonly Func<T> currentValueFunc;
        public MaximumValueHealthCheck(T maximunValue, Func<T> currentValueFunc)
        {
            this.maximunValue = maximunValue;
            this.currentValueFunc = currentValueFunc;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var currentValue = currentValueFunc();

            if (currentValue.CompareTo(maximunValue) <= 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(
                new HealthCheckResult(context.Registration.FailureStatus, description: $"Maximun={maximunValue}, Current={currentValue}"));
        }
    }
}
