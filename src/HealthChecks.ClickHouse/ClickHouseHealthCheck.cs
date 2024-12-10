using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ClickHouse;

/// <summary>
/// A health check for ClickHouse databases.
/// </summary>
public class ClickHouseHealthCheck : IHealthCheck
{
    private readonly ClickHouseHealthCheckOptions _options;

    public ClickHouseHealthCheck(ClickHouseHealthCheckOptions options)
    {
        Guard.ThrowIfNull(options.CommandText, true);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _options.ConnectionFactory();
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
