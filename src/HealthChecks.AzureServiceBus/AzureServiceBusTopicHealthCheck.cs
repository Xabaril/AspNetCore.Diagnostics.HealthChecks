using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusTopicHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ServiceBusAdministrationClient> _managementClientConnections
            = new ConcurrentDictionary<string, ServiceBusAdministrationClient>();


        private readonly string _connectionString;
        private readonly string _topicName;

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }


            _connectionString = connectionString;
            _topicName = topicName;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{_connectionString}_{_topicName}";
                if (!_managementClientConnections.TryGetValue(connectionKey, out var managementClient))
                {
                    managementClient = new ServiceBusAdministrationClient(_connectionString);

                    if (!_managementClientConnections.TryAdd(connectionKey, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New service bus administration client can't be added into dictionary.");
                    }
                }

                _ = await managementClient.GetTopicRuntimePropertiesAsync(_topicName, cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
