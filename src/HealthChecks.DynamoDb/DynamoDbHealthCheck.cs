using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DynamoDb
{
    public class DynamoDbHealthCheck : IHealthCheck
    {
        private readonly DynamoDBOptions _options;

        public DynamoDbHealthCheck(DynamoDBOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (options.RegionEndpoint == null)
                throw new ArgumentNullException(nameof(DynamoDBOptions.RegionEndpoint));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                AWSCredentials credentials = _options.Credentials;
                if (credentials == null)
                {
                    if (!string.IsNullOrEmpty(_options.AccessKey) && !string.IsNullOrEmpty(_options.SecretKey))
                    {
                        // for backwards compatibility we create the basic credentials if the old fields are used
                        // but if they are not specified we fallback to using the default profile
                        credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                    }
                }

                var client = new AmazonDynamoDBClient(credentials, _options.RegionEndpoint);

                _ = await client.ListTablesAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
