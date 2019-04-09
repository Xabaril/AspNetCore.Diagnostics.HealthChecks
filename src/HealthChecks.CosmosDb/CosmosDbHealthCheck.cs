using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace HealthChecks.CosmosDb
{
    public class CosmosDbHealthCheck 
        : IHealthCheck
    {
        private readonly string _connectionString;
        public CosmosDbHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            { 
                using (var cosmosDbClient = new CosmosClient(_connectionString))
                {
                    await cosmosDbClient.GetAccountSettingsAsync();
                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
