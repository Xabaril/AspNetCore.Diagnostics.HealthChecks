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
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new AmazonDynamoDBClient();
                var keysProvided = !string.IsNullOrEmpty(_options.AccessKey) &&
                                   !string.IsNullOrEmpty(_options.SecretKey);
                var regionProvided = _options.RegionEndpoint == null;
                
                if (keysProvided)
                {
                    var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                    client = regionProvided ? new AmazonDynamoDBClient(credentials, _options.RegionEndpoint) : new AmazonDynamoDBClient(credentials);
                }
                else if(regionProvided)
                {
                    client = new AmazonDynamoDBClient( _options.RegionEndpoint);
                }

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
