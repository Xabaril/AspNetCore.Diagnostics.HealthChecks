using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Sqlite;

/// <summary>
/// A health check for Sqlite services.
/// </summary>
public class SqliteHealthCheck : IHealthCheck
{
    private readonly SqliteHealthCheckOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(SqliteHealthCheck) },
                    { "health_check.task", "ready" },
                    { "db.system.name", "sqlite" }
    };

    public SqliteHealthCheck(SqliteHealthCheckOptions options)
    {
        Guard.ThrowIfNull(options.ConnectionString, true);
        Guard.ThrowIfNull(options.CommandText, true);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            using var connection = new SqliteConnection(_options.ConnectionString);
            checkDetails.Add("db.query.text", _options.CommandText);
            checkDetails.Add("db.namespace", connection.Database);
            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = _options.CommandText;
            object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return _options.HealthCheckResultBuilder == null
                ? HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails))
                : _options.HealthCheckResultBuilder(result);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
