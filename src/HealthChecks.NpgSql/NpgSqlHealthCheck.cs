using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.NpgSql
{
    public class NpgSqlHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        public NpgSqlHealthCheck(string npgsqlConnectionString, string sql)
        {
            _connectionString = npgsqlConnectionString ?? throw new ArgumentNullException(nameof(npgsqlConnectionString));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        await command.ExecuteScalarAsync();
                    }

                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}