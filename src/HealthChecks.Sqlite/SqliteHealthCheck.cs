using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Sqlite
{
    public class SqliteHealthCheck 
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        private readonly ILogger<SqliteHealthCheck> _logger;

        public SqliteHealthCheck(string connectionString, string sql, ILogger<SqliteHealthCheck> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _sql = sql ?? throw new ArgumentException(nameof(sql));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    _logger?.LogInformation($"{nameof(SqliteHealthCheck)} is checking the Sqlite using the query {_sql}.");

                    await connection.OpenAsync(cancellationToken);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        await command.ExecuteScalarAsync();
                    }

                    _logger?.LogInformation($"The {nameof(SqliteHealthCheck)} check success for {_connectionString}");

                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(SqliteHealthCheck)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }

       
    }
}
