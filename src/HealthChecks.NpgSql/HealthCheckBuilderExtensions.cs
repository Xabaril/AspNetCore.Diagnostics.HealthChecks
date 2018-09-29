using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "npgsql";

        public static IHealthChecksBuilder AddNpgSql(this IHealthChecksBuilder builder, string npgsqlConnectionString, string healthQuery = "SELECT 1;")
        {
            if (string.IsNullOrWhiteSpace(healthQuery))
            {
                throw new ArgumentNullException(nameof(healthQuery));
            }

            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new NpgSqlHealthCheck(npgsqlConnectionString, healthQuery, sp.GetService<ILogger<NpgSqlHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
