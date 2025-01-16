using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Neo4jClient;

/// <summary>
/// A health check for Neo4j databases.
/// </summary>
public class Neo4jClientHealthCheck : IHealthCheck
{
    ///<inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
