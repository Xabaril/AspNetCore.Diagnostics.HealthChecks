using System.Collections.ObjectModel;
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
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                { "health_check.name", nameof(NatsHealthCheck) },
                { "health_check.task", "ready" },
                { "messaging.system", "nats" }
    };

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            checkDetails.Add("server.address", _natsConnection.ServerInfo?.Host ?? "");
            checkDetails.Add("server.port", _natsConnection.ServerInfo?.Port ?? 0);
            await _natsConnection.ConnectAsync().ConfigureAwait(false);
            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
