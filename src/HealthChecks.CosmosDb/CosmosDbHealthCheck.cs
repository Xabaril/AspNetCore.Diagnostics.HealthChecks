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
        private readonly CosmosDbOptions _cosmosDbOptions = new CosmosDbOptions();
        public CosmosDbHealthCheck(CosmosDbOptions cosmosDbOptions)
        {
            _cosmosDbOptions.UriEndpoint = cosmosDbOptions.UriEndpoint ?? throw new ArgumentNullException(nameof(cosmosDbOptions.UriEndpoint));
            _cosmosDbOptions.PrimaryKey = cosmosDbOptions.PrimaryKey ?? throw new ArgumentNullException(nameof(cosmosDbOptions.PrimaryKey));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            { 
                using (var cosmosDbClient = new CosmosClient(
                    _cosmosDbOptions.UriEndpoint,
                    _cosmosDbOptions.PrimaryKey))
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
