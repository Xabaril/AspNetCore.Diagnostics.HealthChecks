using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.EventStore
{
    public class EventStoreConnectionStringHealthCheck
        : IHealthCheck
    {
        const string CONNECTION_NAME = "AspNetCore ConnectionString HealthCheck Connection";
        private readonly string _eventStoreConnectionString;
        public EventStoreConnectionStringHealthCheck(string eventStoreConnectionString)
        {
            _eventStoreConnectionString = eventStoreConnectionString ?? throw new ArgumentNullException(nameof(eventStoreConnectionString));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = EventStoreConnection.Create(_eventStoreConnectionString, CONNECTION_NAME))
                {
                    await connection.ConnectAsync();
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
