using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.DynamoDb
{
    public class DynamoDbHealthCheck
        : IHealthCheck
    {
        private readonly DynamoDBOptions _options;
        private readonly ILogger<DynamoDbHealthCheck> _logger;

        public DynamoDbHealthCheck(DynamoDBOptions options, ILogger<DynamoDbHealthCheck> logger = null)
        {
            if (string.IsNullOrEmpty(options.AccessKey)) throw new ArgumentNullException(nameof(DynamoDBOptions.AccessKey));
            if (string.IsNullOrEmpty(options.SecretKey)) throw new ArgumentNullException(nameof(DynamoDBOptions.SecretKey));
            if (options.RegionEndpoint == null) throw new ArgumentNullException(nameof(DynamoDBOptions.RegionEndpoint));

            _options = options;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug($"{nameof(DynamoDbHealthCheck)} is checking DynamoDb database availability.");

                var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                var client = new AmazonDynamoDBClient(credentials, _options.RegionEndpoint);

                await client.ListTablesAsync();

                _logger?.LogDebug($"The {nameof(DynamoDbHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"The {nameof(DynamoDbHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
