using HealthChecks.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.SqlServer
{
    public class sqlite_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("sqlite", typeof(SqliteHealthCheck), builder => builder.AddSqlite("connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-sqlite", typeof(SqliteHealthCheck), builder => builder.AddSqlite("connectionstring", name: "my-sqlite"));
        }
    }
}
