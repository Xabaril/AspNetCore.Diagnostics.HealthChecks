using HealthChecks.RavenDB;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
#pragma warning disable 618

namespace UnitTests.DependencyInjection.RavenDB
{
    public class ravendb_with_single_conection_string_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("ravendb", typeof(RavenDBHealthCheck), builder => builder.AddRavenDB("http://localhost:8080"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-ravendb", typeof(RavenDBHealthCheck), builder => builder.AddRavenDB("http://localhost:8080", name: "my-ravendb"));
        }
    }
}
