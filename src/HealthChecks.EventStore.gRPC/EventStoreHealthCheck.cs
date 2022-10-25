using EventStore.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore.gRPC;

/// <summary>
/// Checks whether a gRPC connection can be made to <see cref="EventStore"/>, using the supplied <c>connectionString</c>
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
    /// <param name="context">Health check context. Provides the <see cref="HealthCheckRegistration"/>, which contains information regarding the registration of the <see cref="EventStoreHealthCheck"/>.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <returns><c>Healthy</c> if a connection can be made, otherwise <c>Unhealthy</c></returns>
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
