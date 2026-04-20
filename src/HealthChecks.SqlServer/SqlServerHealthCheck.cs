using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SqlServer;

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
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "db.system.name", "microsoft.sql_server" },
            { "network.transport", "tcp" }
        };

        try
        {
            checkDetails.Add("db.query.text", _options.CommandText);
            using var connection = new SqlConnection(_options.ConnectionString);
            checkDetails.Add("db.namespace", connection.Database);
            checkDetails.Add("server.address", connection.DataSource);

            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            checkDetails.Add("db.client.connection.state", connection.State);
            command.CommandText = _options.CommandText;
            object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return _options.HealthCheckResultBuilder == null
                ? HealthCheckResult.Healthy(data: checkDetails)
                : _options.HealthCheckResultBuilder(result);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
