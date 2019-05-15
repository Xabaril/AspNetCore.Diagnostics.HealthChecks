using HealthChecks.MySql;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.MySql
{
    public class my_sql_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("mysql", typeof(MySqlHealthCheck), builder => builder.AddMySql(
                "connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-mysql-group", typeof(MySqlHealthCheck), builder => builder.AddMySql(
                "connectionstring", name: "my-mysql-group"));
        }
    }
}
