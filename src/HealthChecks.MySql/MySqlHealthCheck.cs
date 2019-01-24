using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        private readonly TimeSpan _timeout;

        public MySqlHealthCheck(string connectionString, TimeSpan timeout)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        await connection.OpenAsync(timeoutCancellationTokenSource.Token);

                        if (!await connection.PingAsync(timeoutCancellationTokenSource.Token))
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"The {nameof(MySqlHealthCheck)} check fail.");
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
