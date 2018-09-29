using HealthChecks.Oracle;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "oracle";

        public static IHealthChecksBuilder AddOracle(this IHealthChecksBuilder builder, string connectionString, string healthQuery = "select * from v$version")
        {
            if (string.IsNullOrEmpty(healthQuery))
            {
                throw new ArgumentNullException(nameof(healthQuery));
            }

            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new OracleHealthCheck(connectionString, healthQuery, sp.GetService<ILogger<OracleHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
