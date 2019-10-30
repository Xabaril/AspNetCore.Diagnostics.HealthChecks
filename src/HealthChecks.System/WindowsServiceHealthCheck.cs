using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class WindowsServiceHealthCheck : IHealthCheck
    {
        private readonly string _serviceName;
        private readonly Func<ServiceController, bool> _predicate;


        public WindowsServiceHealthCheck(string serviceName, Func<ServiceController, bool> predicate)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _predicate = predicate;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var sc = new ServiceController(_serviceName))
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
    }
}