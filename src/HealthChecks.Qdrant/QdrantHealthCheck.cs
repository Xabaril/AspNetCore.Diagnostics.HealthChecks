using Microsoft.Extensions.Diagnostics.HealthChecks;
using Qdrant.Client;

namespace HealthChecks.Qdrant;

/// <summary>
/// A health check for Qdrant services.
/// </summary>
public class QdrantHealthCheck : IHealthCheck
{
    private readonly QdrantClient _client;

    public QdrantHealthCheck(QdrantClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.HealthAsync(cancellationToken).ConfigureAwait(false);
            return response?.Title is not null
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(HealthStatus.Unhealthy);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
