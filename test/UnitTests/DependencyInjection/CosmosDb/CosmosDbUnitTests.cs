using HealthChecks.CosmosDb;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.CosmosDb
{
    public class cosmosdb_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("cosmosdb", typeof(CosmosDbHealthCheck), builder => builder.AddCosmosDb(
                "myconnectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-cosmosdb-group", typeof(CosmosDbHealthCheck), builder => builder.AddCosmosDb(
                "myconnectionstring", name: "my-cosmosdb-group"));
        }
    }
}
