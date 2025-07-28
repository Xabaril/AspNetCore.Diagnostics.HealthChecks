using System.Collections.ObjectModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql;

/// <summary>
/// A health check for MySQL databases.
/// </summary>
public class MySqlHealthCheck : IHealthCheck
{
    private readonly MySqlHealthCheckOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.task", "ready" },
                    { "db.system.name", "mysql" },
                    { "network.transport", "tcp" }
    };

    public MySqlHealthCheck(MySqlHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            using var connection = _options.DataSource is not null ?
                _options.DataSource.CreateConnection() :
                new MySqlConnection(_options.ConnectionString);
            checkDetails.Add("db.namespace", connection.Database);
            checkDetails.Add("server.address", connection.DataSource);

            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            if (_options.CommandText is { } commandText)
            {
                using var command = connection.CreateCommand();
                command.CommandText = _options.CommandText;
                checkDetails.Add("db.query.text", _options.CommandText);
                object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                return _options.HealthCheckResultBuilder == null
                    ? HealthCheckResult.Healthy()
                    : _options.HealthCheckResultBuilder(result);
            }
            else
            {
                var success = await connection.PingAsync(cancellationToken).ConfigureAwait(false);
                return _options.HealthCheckResultBuilder is null
                    ? (success ? HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails)) : new HealthCheckResult(context.Registration.FailureStatus, data: new ReadOnlyDictionary<string, object>(checkDetails))) :
                    _options.HealthCheckResultBuilder(success);
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
