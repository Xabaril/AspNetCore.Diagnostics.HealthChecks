using HealthChecks.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.SqlServer
{
    public class sql_server_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("sqlserver", typeof(SqlServerHealthCheck), builder => builder.AddSqlServer(
                "connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-sql-server-1", typeof(SqlServerHealthCheck), builder => builder.AddSqlServer(
                "connectionstring", name: "my-sql-server-1"));
        }
    }
}
