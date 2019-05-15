using HealthChecks.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.MongoDb
{
    public class mongodb_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured_connectionString()
        {
            ShouldPass("mongodb", typeof(MongoDbHealthCheck), builder => builder.AddMongoDb(
                "mongodb://connectionstring"));
        }
        [Fact]
        public void add_health_check_when_properly_configured_mongoClientSettings()
        {
            ShouldPass("mongodb", typeof(MongoDbHealthCheck), builder => builder.AddMongoDb(
                MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring"))));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured_connectionString()
        {
            ShouldPass("my-mongodb-group", typeof(MongoDbHealthCheck), builder => builder.AddMongoDb(
                   "mongodb://connectionstring", name: "my-mongodb-group"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured_mongoClientSettings()
        {
            ShouldPass("my-mongodb-group", typeof(MongoDbHealthCheck), builder => builder.AddMongoDb(
               MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")), name: "my-mongodb-group"));
        }
    }
}
