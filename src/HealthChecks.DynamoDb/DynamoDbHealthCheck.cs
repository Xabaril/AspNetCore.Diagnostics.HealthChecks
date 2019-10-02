using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.DynamoDb
{
    public class DynamoDbHealthCheck
        : IHealthCheck
    {
        private readonly DynamoDBOptions _options;

        public DynamoDbHealthCheck(DynamoDBOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.ServiceUrl) && options.RegionEndpoint == null)
                throw new ArgumentException("No RegionEndpoint or ServiceUrl are provided");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetDynamoDbClient();
                await client.ListTablesAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private AmazonDynamoDBClient GetDynamoDbClient()
        {
            // ServiceUrl will always take precedence when setting both ServiceUrl and RegionEndpoint
            var config = string.IsNullOrWhiteSpace(_options.ServiceUrl)
                ? new AmazonDynamoDBConfig {RegionEndpoint = _options.RegionEndpoint}
                : new AmazonDynamoDBConfig {ServiceURL = _options.ServiceUrl};

            // if either AccessKey or SecretKey is not provided, don't use basic credentials
            // and let AWS SDK looks up credentials from its credentials provider chain
            // Reference: https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-creds.html#creds-assign
            if (string.IsNullOrWhiteSpace(_options.AccessKey) || string.IsNullOrWhiteSpace(_options.SecretKey))
                return new AmazonDynamoDBClient(config);

            var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
            return new AmazonDynamoDBClient(credentials, config);
        }
    }
}