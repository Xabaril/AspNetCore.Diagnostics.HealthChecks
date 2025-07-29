using System.Collections.ObjectModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DynamoDb;

/// <summary>
/// Health check for AWS DynamoDb database.
/// </summary>
public class DynamoDbHealthCheck : IHealthCheck
{
    private readonly DynamoDBOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(DynamoDbHealthCheck) },
                    { "health_check.task", "ready" },
                    { "db.system.name", "dynamodb" },
                    { "network.transport", "tcp" }
    };

    /// <summary>
    /// Creates health check for AWS DynamoDb database with the specified options.
    /// </summary>
    /// <param name="options"></param>
    public DynamoDbHealthCheck(DynamoDBOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
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
                ? CreateClientWithCredentials(credentials)
                : CreateClientWithoutCredentials();

            var request = new ListTablesRequest { ExclusiveStartTableName = _options.LastEvaluatedTableName };
            if (_options.Limit != null)
                request.Limit = _options.Limit.Value;

            var response = await client.ListTablesAsync(request, cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }

    private AmazonDynamoDBClient CreateClientWithCredentials(AWSCredentials credentials)
    {
        return _options.RegionEndpoint is null
            ? new(credentials)
            : new(credentials, _options.RegionEndpoint);
    }

    private AmazonDynamoDBClient CreateClientWithoutCredentials()
    {
        return _options.RegionEndpoint is null
          ? new()
          : new(_options.RegionEndpoint);

    }
}
