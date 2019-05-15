using HealthChecks.NpgSql;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Npgsql
{
    public class npgsql_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("npgsql", typeof(NpgSqlHealthCheck), builder => builder.AddNpgSql("connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-npg-1", typeof(NpgSqlHealthCheck), builder => builder.AddNpgSql("connectionstring", name: "my-npg-1"));
        }
    }
}
