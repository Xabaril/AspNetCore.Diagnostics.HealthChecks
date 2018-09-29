using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.MongoDb
{
    public class MongoDbHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly ILogger<MongoDbHealthCheck> _logger;

        public MongoDbHealthCheck(string connectionString, ILogger<MongoDbHealthCheck> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(MongoDbHealthCheck)} is checking the MongoDb database.");

                await new MongoClient(_connectionString)
                    .ListDatabasesAsync(cancellationToken);

                _logger?.LogInformation($"The {nameof(MongoDbHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(MongoDbHealthCheck)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}