using System.Collections.ObjectModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace HealthChecks.SurrealDb;

/// <summary>
/// A health check for SurrealDb services.
/// </summary>
public class SurrealDbHealthCheck : IHealthCheck
{
    private readonly ISurrealDbClient _client;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                { "health_check.name", nameof(SurrealDbHealthCheck) },
                { "health_check.task", "ready" },
                { "db.system.name", "surrealdb" }
    };

    public SurrealDbHealthCheck(ISurrealDbClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            checkDetails.Add("server.address", _client.Uri.Host);
            checkDetails.Add("server.port", _client.Uri.Port);

            return await _client.Health(cancellationToken).ConfigureAwait(false)
                ? HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails))
                : HealthCheckResult.Unhealthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
