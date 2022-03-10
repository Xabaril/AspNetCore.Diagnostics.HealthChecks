using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql
{
    public class MySqlHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public MySqlHealthCheck(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    return await connection.PingAsync(cancellationToken)
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
