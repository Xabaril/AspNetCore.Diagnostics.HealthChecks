using Amazon;
using HealthChecks.DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.DynamoDb
{
    public class dynamoDb_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("dynamodb", typeof(DynamoDbHealthCheck), builder => builder.AddDynamoDb(_ =>
                { _.AccessKey = "key"; _.SecretKey = "key"; _.RegionEndpoint = RegionEndpoint.CNNorth1; }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-dynamodb-group", typeof(DynamoDbHealthCheck), builder => builder.AddDynamoDb(_ =>
                { _.AccessKey = "key"; _.SecretKey = "key"; _.RegionEndpoint = RegionEndpoint.CNNorth1; }, name: "my-dynamodb-group"));
        }
    }
}
