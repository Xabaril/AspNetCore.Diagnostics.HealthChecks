using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace HealthChecks.SurrealDb;

/// <summary>
/// A health check for SurrealDb services.
/// </summary>
public class SurrealDbHealthCheck : IHealthCheck
{
    private readonly ISurrealDbClient _client;

    public SurrealDbHealthCheck(ISurrealDbClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "db.system.name", "surrealdb" }
        };

        try
        {
            checkDetails.Add("server.address", _client.Uri.Host);
            checkDetails.Add("server.port", _client.Uri.Port);

            return await _client.Health(cancellationToken).ConfigureAwait(false)
                ? HealthCheckResult.Healthy(data: checkDetails)
                : HealthCheckResult.Unhealthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
