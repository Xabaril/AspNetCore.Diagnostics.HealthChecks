using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Oracle
{
    public class OracleHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        public OracleHealthCheck(string connectionString, string sql)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        await command.ExecuteScalarAsync(cancellationToken);
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
