using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusSubscriptionHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ManagementClient> ManagementClientConnections =
            new ConcurrentDictionary<string, ManagementClient>();
        private readonly string _connectionString;
        private readonly string _topicName;
        private readonly string _subscriptionName;

        public AzureServiceBusSubscriptionHealthCheck(string connectionString, string topicName, string subscriptionName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            if (string.IsNullOrEmpty(subscriptionName))
            {
                throw new ArgumentNullException(nameof(subscriptionName));
            }

            _connectionString = connectionString;
            _topicName = topicName;
            _subscriptionName = subscriptionName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{_connectionString}_{_topicName}_{_subscriptionName}";
                if (!ManagementClientConnections.TryGetValue(connectionKey, out var managementClient))
                {
                    managementClient = new ManagementClient(_connectionString);
                    if (!ManagementClientConnections.TryAdd(connectionKey, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description:
                            "New management client connection can't be added into dictionary.");
                    }
                }

                await managementClient.GetSubscriptionRuntimeInfoAsync(_topicName, _subscriptionName, cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}