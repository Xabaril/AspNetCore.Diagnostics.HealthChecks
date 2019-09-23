using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class ProcessHealthCheck : IHealthCheck
    {
        private string _processName;
        private Func<Process[], bool> _predicate;
        public ProcessHealthCheck(string processName, Func<Process[], bool> predicate)
        {
            _processName = processName ?? throw new ArgumentNullException(nameof(processName));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {            
            try
            {
                var processes = Process.GetProcessesByName(_processName);
                
                if (_predicate(processes))
                {
                    return Task.FromResult(HealthCheckResult.Healthy());
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, exception: ex));
            }
        
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: null));
        }
    }
}
