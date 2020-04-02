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

        public string EntityPath => ConnectionStringBuilder.EntityPath;
        public Uri Endpoint => ConnectionStringBuilder.Endpoint;
        protected EventHubsConnectionStringBuilder ConnectionStringBuilder { get; }

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

            ConnectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString)
            {
                EntityPath = eventHubName
            };
        }

        public AzureEventHubHealthCheck(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            ConnectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var eventHubConnectionString = ConnectionStringBuilder.ToString();
                if (!_eventHubConnections.TryGetValue(eventHubConnectionString, out var eventHubClient))
                {
                    eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString);

                    if (!_eventHubConnections.TryAdd(eventHubConnectionString, eventHubClient))
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
