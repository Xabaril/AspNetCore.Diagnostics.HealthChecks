using ClickHouse.Client.ADO;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ClickHouse;

/// <summary>
/// A health check for ClickHouse databases.
/// </summary>
public class ClickHouseHealthCheck : IHealthCheck
{
    internal const string HEALTH_QUERY = "SELECT 1;";

    private readonly ClickHouseConnection _connection;
    private readonly string _command;

    public ClickHouseHealthCheck(ClickHouseConnection connection, string command)
    {
        _connection = connection;
        _command = command ?? HEALTH_QUERY;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = _connection.CreateCommand();
            command.CommandText = _command;

            await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
        }
    }
}
