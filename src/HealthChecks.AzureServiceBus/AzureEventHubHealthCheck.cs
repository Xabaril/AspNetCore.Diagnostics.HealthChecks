using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AzureEventHubHealthCheck> _logger;

        public AzureEventHubHealthCheck(string connectionString, string eventHubName, ILogger<AzureEventHubHealthCheck> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _eventHubName = eventHubName ?? throw new ArgumentNullException(nameof(eventHubName));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureEventHubHealthCheck)} is checking the Azure Event Hub.");

                var connectionStringBuilder = new EventHubsConnectionStringBuilder(_connectionString)
                {
                    EntityPath = _eventHubName
                };

                var eventHubClient = EventHubClient
                    .CreateFromConnectionString(connectionStringBuilder.ToString());

                await eventHubClient.GetRuntimeInformationAsync();

                _logger?.LogInformation($"The {nameof(AzureEventHubHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureEventHubHealthCheck)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
