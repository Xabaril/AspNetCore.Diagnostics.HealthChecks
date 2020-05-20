using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ManagementClient> _managementClientConnections = new ConcurrentDictionary<string, ManagementClient>();
        private readonly string _connectionString;
        private readonly string _queueName;


        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{_connectionString}_{_queueName}";
                if (!_managementClientConnections.TryGetValue(connectionKey, out var managementClient))
                {
                    managementClient = new ManagementClient(_connectionString);

                    if (!_managementClientConnections.TryAdd(connectionKey, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New management client connection can't be added into dictionary.");
                    }
                }

                await managementClient.GetQueueRuntimeInfoAsync(_queueName, cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
