using Azure.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb
{
    public class CosmosDbHealthCheck : IHealthCheck
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDbHealthCheckOptions _options;

        public CosmosDbHealthCheck(string connectionString)
            : this(connectionString: connectionString, default, default)
        { }

        public CosmosDbHealthCheck(string connectionString, string database)
            : this(connectionString, database, default)
        { }

        public CosmosDbHealthCheck(string accountEndpoint, TokenCredential tokenCredential, string database)
            : this(accountEndpoint, tokenCredential, database, default)
        { }

        public CosmosDbHealthCheck(string connectionString, string? database, IEnumerable<string>? containers)
            : this(
                  ClientCache.GetOrAddDisposable(connectionString, k => new CosmosClient(connectionString)),
                  new CosmosDbHealthCheckOptions { ContainerIds = containers, DatabaseId = database })
        { }

        public CosmosDbHealthCheck(string accountEndpoint, TokenCredential tokenCredential, string? database, IEnumerable<string>? containers)
            : this(
                  ClientCache.GetOrAddDisposable(accountEndpoint, k => new CosmosClient(accountEndpoint, tokenCredential)),
                  new CosmosDbHealthCheckOptions { ContainerIds = containers, DatabaseId = database })
        { }

        public CosmosDbHealthCheck(CosmosClient cosmosClient, CosmosDbHealthCheckOptions options)
        {
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cosmosClient.ReadAccountAsync();

                if (_options.DatabaseId != null)
                {
                    var database = _cosmosClient.GetDatabase(_options.DatabaseId);
                    await database.ReadAsync();

                    if (_options.ContainerIds != null)
                    {
                        foreach (var container in _options.ContainerIds)
                        {
                            await database.GetContainer(container)
                                .ReadContainerAsync();
                        }
                    }
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
