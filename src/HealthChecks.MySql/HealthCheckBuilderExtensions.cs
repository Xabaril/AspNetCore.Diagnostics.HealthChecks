using HealthChecks.MySql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "mysql";
        public static IHealthChecksBuilder AddMySql(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new MySqlHealthCheck(connectionString, sp.GetService<ILogger<MySqlHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
