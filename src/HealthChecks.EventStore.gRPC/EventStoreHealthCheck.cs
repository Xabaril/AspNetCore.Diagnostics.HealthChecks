using EventStore.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore.gRPC
{
    public class EventStoreHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public EventStoreHealthCheck(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var client = new EventStoreClient(EventStoreClientSettings.Create(_connectionString));

                using var subscription = await client.SubscribeToAllAsync(FromAll.End,
                    eventAppeared: (_, _, _) => Task.CompletedTask,
                    cancellationToken: cancellationToken);

                return new HealthCheckResult(HealthStatus.Healthy);
            }
            catch (Exception exception)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: exception);
            }
        }
    }
}
