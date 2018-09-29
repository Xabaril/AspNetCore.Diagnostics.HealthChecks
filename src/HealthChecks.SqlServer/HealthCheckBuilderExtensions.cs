using HealthChecks.SqlServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string name = "sqlserver";

        public static IHealthChecksBuilder AddSqlServer(this IHealthChecksBuilder builder, string connectionString, string healthQuery = "SELECT 1;")
        {
            if (string.IsNullOrWhiteSpace(healthQuery))
            {
                throw new System.ArgumentNullException(nameof(healthQuery));
            }

            return builder.Add(new HealthCheckRegistration(
               name,
               sp => new SqlServerHealthCheck(connectionString,healthQuery, sp.GetService<ILogger<SqlServerHealthCheck>>()),
               null,
               new string[] { name }));
        }
    }
}
