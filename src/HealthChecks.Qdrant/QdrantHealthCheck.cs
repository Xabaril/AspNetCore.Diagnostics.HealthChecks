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
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "db.system.name", "qdrant" }
        };

        try
        {
            await _client.HealthAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
