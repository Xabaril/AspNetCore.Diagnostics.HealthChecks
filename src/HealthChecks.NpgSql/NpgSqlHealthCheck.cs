using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.NpgSql;

/// <summary>
/// A health check for Postgres databases.
/// </summary>
public class NpgSqlHealthCheck : IHealthCheck
{
    private readonly NpgSqlHealthCheckOptions _options;

    public NpgSqlHealthCheck(NpgSqlHealthCheckOptions options)
    {
        Guard.ThrowIfNull(options.DataSource);
        Guard.ThrowIfNull(options.CommandText, true);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _options.DataSource!.CreateConnection();

            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = _options.CommandText;
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return _options.HealthCheckResultBuilder == null
                ? HealthCheckResult.Healthy()
                : _options.HealthCheckResultBuilder(result);

        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
        }
    }
}
