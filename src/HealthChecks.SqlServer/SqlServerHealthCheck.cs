using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SqlServer
{
    /// <summary>
    /// A health check for SqlServer services.
    /// </summary>
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly SqlServerHealthCheckOptions _options;

        public SqlServerHealthCheck(SqlServerHealthCheckOptions options)
        {
            Guard.ThrowIfNull(options.ConnectionString, true);
            Guard.ThrowIfNull(options.CommandText, true);
            _options = options;
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqlConnection(_options.ConnectionString))
                {
                    _options.Configure?.Invoke(connection);
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    using var command = connection.CreateCommand();
                    command.CommandText = _options.CommandText;
                    var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    return _options.HealthCheckResultBuilder == null
                        ? HealthCheckResult.Healthy()
                        : _options.HealthCheckResultBuilder(result);
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
