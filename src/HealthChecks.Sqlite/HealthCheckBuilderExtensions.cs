using HealthChecks.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "sqlite";

        public static IHealthChecksBuilder AddSqlite(this IHealthChecksBuilder builder, string sqliteConnectionString,string healthQuery = "select name from sqlite_master where type='table'")
        {
            if (string.IsNullOrWhiteSpace(healthQuery))
            {
                throw new System.ArgumentNullException(nameof(healthQuery));
            }


            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new SqliteHealthCheck(sqliteConnectionString,healthQuery, sp.GetService<ILogger<SqliteHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
