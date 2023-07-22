using Amazon;
using Amazon.Runtime;

namespace HealthChecks.DynamoDb;

/// <summary>
/// Options for <see cref="DynamoDbHealthCheck"/>.
/// </summary>
public class DynamoDBOptions
{
    public AWSCredentials? Credentials { get; set; }

    [Obsolete("Specify access key and secret as a BasicCredential to Credentials property instead")]
    public string? AccessKey { get; set; }

    [Obsolete("Specify access key and secret as a BasicCredential to Credentials property instead")]
    public string? SecretKey { get; set; }

    public RegionEndpoint RegionEndpoint { get; set; } = null!;

    /// <summary>
    /// A maximum number of table names to read.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// The first table name to read.
    /// </summary>
    public string? LastEvaluatedTableName { get; set; }
}
