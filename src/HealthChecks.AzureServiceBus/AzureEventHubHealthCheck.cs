using Microsoft.Azure.EventHubs;
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
        private static readonly ConcurrentDictionary<string, EventHubClient> _eventHubConnections = new ConcurrentDictionary<string, EventHubClient>();

        private readonly string _eventHubConnectionString;
        public AzureEventHubHealthCheck(string connectionString, string eventHubName)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (eventHubName == null)
            {
                throw new ArgumentNullException(nameof(eventHubName));
            }

            if (connectionString != "" && eventHubName != "")
            {
                var builder = new EventHubsConnectionStringBuilder(connectionString)
                {
                    EntityPath = eventHubName
                };
                _eventHubConnectionString = builder.ToString();
            }
            else
            {
                _eventHubConnectionString = "";
            }
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_eventHubConnections.TryGetValue(_eventHubConnectionString, out var eventHubClient))
                {
                    eventHubClient = EventHubClient.CreateFromConnectionString(_eventHubConnectionString);

                    if (!_eventHubConnections.TryAdd(_eventHubConnectionString, eventHubClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "EventHubClient can't be added into dictionary.");
                    }
                }

                await eventHubClient.GetRuntimeInformationAsync();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
