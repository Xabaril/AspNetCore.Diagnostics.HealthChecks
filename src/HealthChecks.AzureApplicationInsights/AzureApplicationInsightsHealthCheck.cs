using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureApplicationInsights
{
    public class AzureApplicationInsightsHealthCheck : IHealthCheck
    {
        private readonly string _instrumentationKey;
        public AzureApplicationInsightsHealthCheck(string instrumentationKey)
        {
            if(string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey));
            }

            _instrumentationKey = instrumentationKey;         
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
