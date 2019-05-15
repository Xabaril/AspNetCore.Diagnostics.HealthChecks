using HealthChecks.Oracle;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Oracle
{
    public class oracle_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("oracle", typeof(OracleHealthCheck), builder => builder.AddOracle("connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-oracle-1", typeof(OracleHealthCheck), builder => builder.AddOracle("connectionstring", name: "my-oracle-1"));
        }
    }
}
