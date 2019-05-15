using HealthChecks.RavenDB;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.RavenDB
{
    public class ravendb_with_options_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("ravendb", typeof(RavenDBHealthCheck), builder => builder.AddRavenDB(
                _ => { _.Urls = new[] { "http://localhost:8080", "http://localhost:8081" }; }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-ravendb", typeof(RavenDBHealthCheck), builder => builder.AddRavenDB(
                   _ => { _.Urls = new[] { "http://localhost:8080", "http://localhost:8081" }; }, name: "my-ravendb"));
        }
    }
}
