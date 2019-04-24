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
        private readonly T _maximumValue;
        private readonly Func<T> _currentValueFunc;
        public MaximumValueHealthCheck(T maximumValue, Func<T> currentValueFunc)
        {
            _maximumValue = maximumValue;
            _currentValueFunc = currentValueFunc ?? throw new ArgumentNullException(nameof(currentValueFunc));
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var currentValue = _currentValueFunc();

            if (currentValue.CompareTo(_maximumValue) <= 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(
                new HealthCheckResult(context.Registration.FailureStatus, description: $"Maximum={_maximumValue}, Current={currentValue}"));
        }
    }
}
