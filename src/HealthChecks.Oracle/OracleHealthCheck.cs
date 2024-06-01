using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oracle.ManagedDataAccess.Client;

namespace HealthChecks.Oracle;

/// <summary>
/// A health check for Oracle databases.
/// </summary>
public class OracleHealthCheck : IHealthCheck
{
    private readonly OracleHealthCheckOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(OracleHealthCheck) },
                    { "health_check.task", "ready" },
                    { "db.system.name", "oracle" }
    };

    public OracleHealthCheck(OracleHealthCheckOptions options)
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
            Dictionary<string, object> checkDetails = _baseCheckDetails;
            using var connection = _options.Credential == null
                ? new OracleConnection(_options.ConnectionString)
                : new OracleConnection(_options.ConnectionString, _options.Credential);
            checkDetails.Add("db.query.text", _options.CommandText);
            checkDetails.Add("db.namespace", connection.Database);
            checkDetails.Add("server.address", connection.DataSource);

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
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
