using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureEventHubHealthCheck
        : IHealthCheck
    {
        const string EntityPathSegment = "EntityPath=";

        private static readonly ConcurrentDictionary<string, EventHubProducerClient> _eventHubConnections = new ConcurrentDictionary<string, EventHubProducerClient>();
        private readonly string _eventHubConnectionString;

        public AzureEventHubHealthCheck(string connectionString, string eventHubName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(eventHubName))
            {
                throw new ArgumentNullException(nameof(eventHubName));
            }

            _eventHubConnectionString = connectionString.Contains(EntityPathSegment) ? connectionString : $"{connectionString};{EntityPathSegment}{eventHubName}";
            _eventHubConnections.TryAdd(_eventHubConnectionString, new EventHubProducerClient(_eventHubConnectionString));
        }

        public AzureEventHubHealthCheck(EventHubConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _eventHubConnectionString = $"{connection.FullyQualifiedNamespace};{EntityPathSegment}{connection.EventHubName}";
            _eventHubConnections.TryAdd(_eventHubConnectionString, new EventHubProducerClient(connection));
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
