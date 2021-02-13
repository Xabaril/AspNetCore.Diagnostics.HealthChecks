using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class WindowsServiceHealthCheck : IHealthCheck
    {
        private readonly string _serviceName;
        private readonly Func<ServiceController, bool> _predicate;
        private readonly string _machineName;

        public WindowsServiceHealthCheck(string serviceName, Func<ServiceController, bool> predicate, string machineName = default)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _machineName = machineName;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var sc = GetServiceController())
                {
                    if (_predicate(sc))
                    {
                        return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, exception: null));
                    }
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }

            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: null));
        }

        private ServiceController GetServiceController() =>
            !string.IsNullOrEmpty(_machineName)
                ? new ServiceController(_serviceName, _machineName)
                : new ServiceController(_serviceName);

    }
}