using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace HealthChecks.NpgSql;

/// <summary>
/// A health check for Postgres databases.
/// </summary>
public class NpgSqlHealthCheck : IHealthCheck
{
    private readonly NpgSqlHealthCheckOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.task", "ready" },
                    { "db.system.name", "npgsql" },
                    { "network.transport", "tcp" }
    };

    public NpgSqlHealthCheck(NpgSqlHealthCheckOptions options)
    {
        Debug.Assert(options.ConnectionString is not null || options.DataSource is not null);
        Guard.ThrowIfNull(options.CommandText, true);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            await using var connection = _options.DataSource is not null
                ? _options.DataSource.CreateConnection()
                : new NpgsqlConnection(_options.ConnectionString);

            checkDetails.Add("db.query.text", _options.CommandText);
            checkDetails.Add("db.namespace", connection.Database);
            checkDetails.Add("server.address", connection.DataSource);
            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = _options.CommandText;
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return _options.HealthCheckResultBuilder == null
                ? HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails))
                : _options.HealthCheckResultBuilder(result);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
