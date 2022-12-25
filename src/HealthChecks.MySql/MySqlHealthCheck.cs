using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql
{
    /// <summary>
    /// A health check for MySql databases.
    /// </summary>
    public class MySqlHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public MySqlHealthCheck(string connectionString)
        {
            _connectionString = Guard.ThrowIfNull(connectionString);
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    return await connection.PingAsync(cancellationToken).ConfigureAwait(false)
                        ? HealthCheckResult.Healthy()
                        : new HealthCheckResult(context.Registration.FailureStatus, description: $"The {nameof(MySqlHealthCheck)} check fail.");
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
