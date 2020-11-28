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
        private readonly Func<object, HealthCheckResult> _queryValidator;

        public SqlServerHealthCheck(string sqlserverconnectionstring, string sql, Action<SqlConnection> beforeOpenConnectionConfigurer = null, Func<object, HealthCheckResult> queryValidator = null)
        {
            _connectionString = sqlserverconnectionstring ?? throw new ArgumentNullException(nameof(sqlserverconnectionstring));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
            _beforeOpenConnectionConfigurer = beforeOpenConnectionConfigurer;
            _queryValidator = queryValidator;
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
                        var queryResult = await command.ExecuteScalarAsync(cancellationToken);
                        if (_queryValidator != null)
                        {
                            return _queryValidator(queryResult);
                        }
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
