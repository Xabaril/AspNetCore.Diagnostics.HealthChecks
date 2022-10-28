using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SqlServer
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        private readonly Action<SqlConnection>? _beforeOpenConnectionConfigurer;

        public SqlServerHealthCheck(string sqlserverconnectionstring, string sql, Action<SqlConnection>? beforeOpenConnectionConfigurer = null)
        {
            _connectionString = Guard.ThrowIfNull(sqlserverconnectionstring);
            _sql = Guard.ThrowIfNull(sql);
            _beforeOpenConnectionConfigurer = beforeOpenConnectionConfigurer;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _beforeOpenConnectionConfigurer?.Invoke(connection);

                    await connection.OpenAsync(cancellationToken);
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        _ = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
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
