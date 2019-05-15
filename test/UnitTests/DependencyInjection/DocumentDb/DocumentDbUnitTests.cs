using HealthChecks.DocumentDb;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.DocumentDb
{
    public class documentdb_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("documentdb", typeof(DocumentDbHealthCheck), builder => builder.AddDocumentDb(_ => 
                { _.PrimaryKey = "key"; _.UriEndpoint = "endpoint"; }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-documentdb-group", typeof(DocumentDbHealthCheck), builder => builder.AddDocumentDb(_ =>
                { _.PrimaryKey = "key"; _.UriEndpoint = "endpoint"; }, name: "my-documentdb-group"));
        }
    }
}
