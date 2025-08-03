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
        try
        {
            return await _client.Health(cancellationToken).ConfigureAwait(false)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
