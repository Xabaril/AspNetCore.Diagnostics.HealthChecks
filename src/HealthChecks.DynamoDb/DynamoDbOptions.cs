using Amazon;

namespace HealthChecks.DynamoDb
{
    public class DynamoDBOptions
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public RegionEndpoint RegionEndpoint { get; set; }
    }
}
