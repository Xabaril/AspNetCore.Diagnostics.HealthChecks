using Microsoft.Azure.ServiceBus.Management;
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
        private static readonly ConcurrentDictionary<string, ManagementClient> _managementClients = new ConcurrentDictionary<string, ManagementClient>();

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
                if (!_managementClients.TryGetValue(connectionKey, out var managementClient))
                {
                    managementClient = new ManagementClient(_connectionString);

                    if (!_managementClients.TryAdd(connectionKey, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New Management Client can't be added into dictionary.");
                    }
                }

                await managementClient.GetTopicRuntimeInfoAsync(_topicName);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
