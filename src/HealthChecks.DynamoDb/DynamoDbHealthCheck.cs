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
            _options = Guard.ThrowIfNull(options);

            Guard.ThrowIfNull(options.RegionEndpoint);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                AWSCredentials? credentials = _options.Credentials;

                if (credentials == null)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (!string.IsNullOrEmpty(_options.AccessKey) && !string.IsNullOrEmpty(_options.SecretKey))
                    {
                        // for backwards compatibility we create the basic credentials if the old fields are used
                        // but if they are not specified we fallback to using the default profile
                        credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                    }
#pragma warning restore CS0618 // Type or member is obsolete
                }

                using var client = credentials != null
                    ? new AmazonDynamoDBClient(credentials, _options.RegionEndpoint)
                    : new AmazonDynamoDBClient(_options.RegionEndpoint);

                _ = await client.ListTablesAsync(cancellationToken).ConfigureAwait(false);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
