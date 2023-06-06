using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql;

/// <summary>
/// A health check for MySql databases.
/// </summary>
public class MySqlHealthCheck : IHealthCheck
{
    private readonly MySqlHealthCheckOptions _options;

    public MySqlHealthCheck(MySqlHealthCheckOptions options)
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
            using var connection = new MySqlConnection(_options.ConnectionString);

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();

            command.CommandText = _options.CommandText;

            object? result = await command
                .ExecuteScalarAsync(cancellationToken)
                .ConfigureAwait(false);

            var returnQueryResults =
                _options.HealthCheckResultBuilder?
                    .Invoke(result) ?? HealthCheckResult.Healthy();

            return await connection.PingAsync(cancellationToken).ConfigureAwait(false)
                ? returnQueryResults
                : new HealthCheckResult(context.Registration.FailureStatus, description: $"The {nameof(MySqlHealthCheck)} check fail.");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
