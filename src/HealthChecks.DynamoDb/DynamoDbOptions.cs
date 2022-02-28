using Amazon;

namespace HealthChecks.DynamoDb
{
    /// <summary>
    /// Options for <see cref="DynamoDbHealthCheck"/>.
    /// </summary>
    public class DynamoDBOptions
    {
        public string AccessKey { get; set; } = null!;

        public string SecretKey { get; set; } = null!;

        public RegionEndpoint RegionEndpoint { get; set; } = null!;
    }
}
