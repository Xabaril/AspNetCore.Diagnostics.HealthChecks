using Microsoft.Extensions.Diagnostics.HealthChecks;
using NATS.Client.Core;

namespace HealthChecks.Nats;

/// <summary>
/// Health check for Nats Server.
/// </summary>
public sealed class NatsHealthCheck(INatsConnection connection) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        await TryConnectAsync(connection).ConfigureAwait(false);

    private static async Task<HealthCheckResult> TryConnectAsync(INatsConnection natsConnection)
    {
        try
        {
            await natsConnection.ConnectAsync().ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
