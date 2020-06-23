using ClickHouse.Ado;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Clickhouse
{
    public class ClickhouseHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        private readonly Action<ClickHouseConnection> _connectionAction;

        public ClickhouseHealthCheck(string clickhouseConnectionString, string sql, Action<ClickHouseConnection> connectionAction = null)
        {
            _connectionString = clickhouseConnectionString ?? throw new ArgumentNullException(nameof(clickhouseConnectionString));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
            _connectionAction = connectionAction;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new ClickHouseConnection(_connectionString))
                {
                    _connectionAction?.Invoke(connection);

                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        command.ExecuteScalar();
                    }

                    return Task.FromResult(HealthCheckResult.Healthy());
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}