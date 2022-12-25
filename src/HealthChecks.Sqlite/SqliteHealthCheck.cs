using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Sqlite
{
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;

        public SqliteHealthCheck(string connectionString, string sql)
        {
            _connectionString = Guard.ThrowIfNull(connectionString);
            _sql = sql ?? throw new ArgumentException(nameof(sql));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
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
