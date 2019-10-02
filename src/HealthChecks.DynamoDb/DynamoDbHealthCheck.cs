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
            if (string.IsNullOrEmpty(options.AccessKey)) throw new ArgumentNullException(nameof(DynamoDBOptions.AccessKey));
            if (string.IsNullOrEmpty(options.SecretKey)) throw new ArgumentNullException(nameof(DynamoDBOptions.SecretKey));
            if (string.IsNullOrWhiteSpace(options.ServiceUrl) && options.RegionEndpoint == null) throw new ArgumentException("No RegionEndpoint or ServiceUrl are provided");
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var config = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = _options.RegionEndpoint,
                    ServiceURL =  _options.ServiceUrl
                };
                var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                var client = new AmazonDynamoDBClient(credentials, config);

                await client.ListTablesAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
