using EventStore.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore.gRPC;

/// <summary>
/// Checks whether a gRPC connection can be made to EventStore services using the supplied connection string.
/// </summary>
public class EventStoreHealthCheck : IHealthCheck, IDisposable
{
    private readonly EventStoreClient _client;

    public EventStoreHealthCheck(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        _client = new EventStoreClient(EventStoreClientSettings.Create(connectionString));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var subscription = await _client.SubscribeToAllAsync(
                FromAll.End,
                eventAppeared: (_, _, _) => Task.CompletedTask,
                cancellationToken: cancellationToken);

            return new HealthCheckResult(HealthStatus.Healthy);
        }
        catch (Exception exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: exception);
        }
    }

    public void Dispose() => _client.Dispose();
}
