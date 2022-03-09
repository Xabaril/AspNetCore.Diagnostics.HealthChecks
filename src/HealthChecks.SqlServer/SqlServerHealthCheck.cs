using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SqlServer
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        private readonly Action<SqlConnection>? _beforeOpenConnectionConfigurer;
        private readonly Func<string>? _accessTokenProvider;

        public SqlServerHealthCheck(string sqlserverconnectionstring, string sql, Action<SqlConnection>? beforeOpenConnectionConfigurer = null, Func<string>? accessTokenProvider = null)
        {
            _connectionString = sqlserverconnectionstring ?? throw new ArgumentNullException(nameof(sqlserverconnectionstring));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
            _beforeOpenConnectionConfigurer = beforeOpenConnectionConfigurer;
            _accessTokenProvider = accessTokenProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString) { AccessToken = _accessTokenProvider?.Invoke() })
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
