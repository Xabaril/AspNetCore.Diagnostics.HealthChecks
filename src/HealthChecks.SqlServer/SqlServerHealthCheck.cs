using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.SqlServer
{
    public class SqlServerHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        private readonly Action<SqlConnection> _beforeOpenConnectionConfigurer;

        public SqlServerHealthCheck(string sqlServerConnectionString, string sql, Action<SqlConnection> beforeOpenConnectionConfigurer = null)
        {
            _connectionString = sqlServerConnectionString ?? throw new ArgumentNullException(nameof(sqlServerConnectionString));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
            _beforeOpenConnectionConfigurer = beforeOpenConnectionConfigurer;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                _beforeOpenConnectionConfigurer?.Invoke(connection);

                await connection.OpenAsync(cancellationToken);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = _sql;
                    await command.ExecuteScalarAsync(cancellationToken);
                }
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
