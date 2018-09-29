using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.MySql
{
    public class MySqlHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly ILogger<MySqlHealthCheck> _logger;

        public MySqlHealthCheck(string connectionString, ILogger<MySqlHealthCheck> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(MySqlHealthCheck)} is checking the MySql.");

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    if (!await connection.PingAsync(cancellationToken))
                    {
                        _logger?.LogWarning($"The {nameof(MySqlHealthCheck)} check fail for {_connectionString}.");

                        return HealthCheckResult.Failed($"The {nameof(MySqlHealthCheck)} check fail.");
                    }

                    _logger?.LogInformation($"The {nameof(MySqlHealthCheck)} check success for {_connectionString}");

                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(MySqlHealthCheck)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
