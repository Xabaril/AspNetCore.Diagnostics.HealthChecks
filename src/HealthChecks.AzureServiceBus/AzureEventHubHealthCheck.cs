using System.Collections.Concurrent;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureEventHubHealthCheck : IHealthCheck
    {
        private const string ENTITY_PATH_SEGMENT = "EntityPath=";

        private static readonly ConcurrentDictionary<string, EventHubProducerClient> _eventHubConnections = new ConcurrentDictionary<string, EventHubProducerClient>();
        private readonly string _eventHubConnectionString;

        public AzureEventHubHealthCheck(string connectionString, string eventHubName)
        {
            Guard.ThrowIfNull(connectionString, true);
            Guard.ThrowIfNull(eventHubName, true);

            _eventHubConnectionString = connectionString.Contains(ENTITY_PATH_SEGMENT) ? connectionString : $"{connectionString};{ENTITY_PATH_SEGMENT}{eventHubName}";

            if (!_eventHubConnections.ContainsKey(_eventHubConnectionString))
            {
                _eventHubConnections.TryAdd(_eventHubConnectionString, new EventHubProducerClient(_eventHubConnectionString));
            }
        }

        public AzureEventHubHealthCheck(EventHubConnection connection)
        {
            Guard.ThrowIfNull(connection);

            _eventHubConnectionString = $"{connection.FullyQualifiedNamespace};{ENTITY_PATH_SEGMENT}{connection.EventHubName}";

            if (!_eventHubConnections.ContainsKey(_eventHubConnectionString))
            {
                _eventHubConnections.TryAdd(_eventHubConnectionString, new EventHubProducerClient(connection));
            }
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_eventHubConnections.TryGetValue(_eventHubConnectionString, out var producer))
                {
                    producer = new EventHubProducerClient(_eventHubConnectionString);

                    if (!_eventHubConnections.TryAdd(_eventHubConnectionString, producer))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "EventHubProducerClient can't be added into dictionary.");
                    }
                }

                _ = await producer.GetEventHubPropertiesAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
