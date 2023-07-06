using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DynamoDb;

public class DynamoDbHealthCheck : IHealthCheck
{
    private readonly DynamoDBOptions _options;

    public DynamoDbHealthCheck(DynamoDBOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
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
                    // for backwards compatibility we create the basic credentials if the old fields are used
                    var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                    client = regionProvided ? new AmazonDynamoDBClient(credentials, _options.RegionEndpoint) : new AmazonDynamoDBClient(credentials);
                }
                else if (regionProvided)

                {
                    client = new AmazonDynamoDBClient( _options.RegionEndpoint);
                }
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
