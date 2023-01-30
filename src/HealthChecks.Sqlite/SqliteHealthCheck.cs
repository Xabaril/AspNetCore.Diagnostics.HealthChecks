using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Sqlite
{
    /// <summary>
    /// A health check for Sqlite services.
    /// </summary>
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly SqliteHealthCheckOptions _options;

        public SqliteHealthCheck(SqliteHealthCheckOptions options)
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
                using var connection = new SqliteConnection(_options.ConnectionString);

                _options.Configure?.Invoke(connection);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using var command = connection.CreateCommand();
                command.CommandText = _options.CommandText;
                object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                return _options.HealthCheckResultBuilder == null
                    ? HealthCheckResult.Healthy()
                    : _options.HealthCheckResultBuilder(result);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
