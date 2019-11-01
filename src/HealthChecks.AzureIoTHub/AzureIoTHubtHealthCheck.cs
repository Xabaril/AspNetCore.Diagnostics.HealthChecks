using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace HealthChecks.AzureIoTHub
{
    public class AzureIoTHubHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _healthQuery;
        public AzureIoTHubHealthCheck(string connectionString, string healthQuery)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _healthQuery = healthQuery ?? throw new ArgumentNullException(nameof(healthQuery));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = RegistryManager.CreateFromConnectionString(_connectionString))
                {
                    var query = client.CreateQuery(_healthQuery, 1);
                    var _ = await query.GetNextAsJsonAsync();
                }
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
