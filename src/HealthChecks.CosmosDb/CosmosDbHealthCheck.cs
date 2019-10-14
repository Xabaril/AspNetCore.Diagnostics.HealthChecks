using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.CosmosDb
{
    public class CosmosDbHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, CosmosClient> _connections = new ConcurrentDictionary<string, CosmosClient>();

        private readonly string _connectionString;
        public CosmosDbHealthCheck(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                CosmosClient cosmosDbClient;

                if (!_connections.TryGetValue(_connectionString, out cosmosDbClient))
                {
                    cosmosDbClient = new CosmosClient(_connectionString);

                    if (!_connections.TryAdd(_connectionString, cosmosDbClient))
                    {
                        cosmosDbClient.Dispose();
                        cosmosDbClient = _connections[_connectionString];
                    }
                }
                await cosmosDbClient.ReadAccountAsync();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}