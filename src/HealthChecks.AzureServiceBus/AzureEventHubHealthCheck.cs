using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureEventHubHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _eventHubName;

        public AzureEventHubHealthCheck(string connectionString, string eventHubName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _eventHubName = eventHubName ?? throw new ArgumentNullException(nameof(eventHubName));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(_connectionString)
                {
                    EntityPath = _eventHubName
                };
                var eventHubClient = EventHubClient
                    .CreateFromConnectionString(connectionStringBuilder.ToString());

                await eventHubClient.GetRuntimeInformationAsync();
                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
