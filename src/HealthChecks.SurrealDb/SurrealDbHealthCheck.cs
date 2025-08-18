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
            { "db.system.name", "surrealdb" },
            { "network.transport", "tcp" }
        };

        try
        {
            checkDetails.Add("server.address", _client.Uri.Host);
            checkDetails.Add("server.port", _client.Uri.Port);

            if (_client.Uri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase) ||
                _client.Uri.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase))
            {
                checkDetails.Add("network.protocol.name", "http");
            }
            else if (_client.Uri.Scheme.Equals("wss", StringComparison.CurrentCultureIgnoreCase) ||
                _client.Uri.Scheme.Equals("ws", StringComparison.CurrentCultureIgnoreCase))
            {
                checkDetails.Add("network.protocol.name", "websocket");
            }

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
