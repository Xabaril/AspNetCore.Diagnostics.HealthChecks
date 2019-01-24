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
        private readonly TimeSpan _timeout;

        public NpgSqlHealthCheck(string npgsqlConnectionString, string sql, TimeSpan timeout)
        {
            _connectionString = npgsqlConnectionString ?? throw new ArgumentNullException(nameof(npgsqlConnectionString));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync(timeoutCancellationTokenSource.Token);

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = _sql;
                            await command.ExecuteScalarAsync(timeoutCancellationTokenSource.Token);
                        }

                        return HealthCheckResult.Healthy();
                    }
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}