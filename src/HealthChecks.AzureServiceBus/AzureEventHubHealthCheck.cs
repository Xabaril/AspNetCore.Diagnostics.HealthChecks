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
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(eventHubName)) throw new ArgumentNullException(nameof(eventHubName));

            _connectionString = connectionString;
            _eventHubName = eventHubName;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(_connectionString)
                {
                    EntityPath = _eventHubName
                };
                var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
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
