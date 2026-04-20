using Microsoft.Extensions.Diagnostics.HealthChecks;
using NATS.Client.Core;

namespace HealthChecks.Nats;

/// <summary>
/// Health check for Nats Server.
/// </summary>
public class NatsHealthCheck : IHealthCheck
{
    private readonly INatsConnection _natsConnection;

    public NatsHealthCheck(INatsConnection connection)
    {
        _natsConnection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "messaging.system", "nats" }
        };

        try
        {
            checkDetails.Add("server.address", _natsConnection.ServerInfo?.Host ?? "");
            checkDetails.Add("server.port", _natsConnection.ServerInfo?.Port ?? 0);
            await _natsConnection.ConnectAsync().ConfigureAwait(false);
            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
