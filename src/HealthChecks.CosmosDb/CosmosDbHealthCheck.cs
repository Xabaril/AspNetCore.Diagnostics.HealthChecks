using System.Collections.Concurrent;
using Azure.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb
{
    public class CosmosDbHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, CosmosClient> _connections = new();

        private readonly string? _connectionString;
        private readonly string? _accountEndpoint;
        private readonly TokenCredential? _tokenCredential;
        private readonly string? _database;
        private readonly IEnumerable<string>? _containers;

        public CosmosDbHealthCheck(string connectionString)
            : this(connectionString: connectionString, default, default)
        {
        }

        public CosmosDbHealthCheck(string connectionString, string database)
            : this(connectionString, database, default)
        {
            _database = database;
        }

        public CosmosDbHealthCheck(string accountEndpoint, TokenCredential tokenCredential, string database)
            : this(accountEndpoint, tokenCredential, database, default)
        {
            _database = database;
        }

        public CosmosDbHealthCheck(string connectionString, string? database, IEnumerable<string>? containers)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _database = database;
            _containers = containers;
        }

        public CosmosDbHealthCheck(string accountEndpoint, TokenCredential tokenCredential, string? database, IEnumerable<string>? containers)
        {
            _accountEndpoint = accountEndpoint ?? throw new ArgumentNullException(nameof(accountEndpoint));
            _tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
            _database = database;
            _containers = containers;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionsKey = _connectionString ?? _accountEndpoint!;

                if (!_connections.TryGetValue(connectionsKey, out var cosmosDbClient))
                {
                    cosmosDbClient = CreateCosmosDbClient();

                    if (!_connections.TryAdd(connectionsKey, cosmosDbClient))
                    {
                        cosmosDbClient.Dispose();
                        cosmosDbClient = _connections[connectionsKey];
                    }
                }

                await cosmosDbClient.ReadAccountAsync();

                if (_database != null)
                {
                    var database = cosmosDbClient.GetDatabase(_database);
                    await database.ReadAsync();

                    if (_containers != null && _containers.Any())
                    {
                        foreach (var container in _containers)
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

        private CosmosClient CreateCosmosDbClient()
        {
            return !string.IsNullOrEmpty(_connectionString)
                    ? new CosmosClient(_connectionString!)
                    : new CosmosClient(_accountEndpoint, _tokenCredential);
        }
    }
}
