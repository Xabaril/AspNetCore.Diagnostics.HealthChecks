using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueMessageThresholdCountHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _queueName;
        private readonly int _degradedThreshold;
        private readonly int _unhealthyThreshold;

        public AzureServiceBusQueueMessageThresholdCountHealthCheck(string connectionString, string queueName, int degradedThreshold = 5, int unhealthyThreshold = 10) 
            : base(connectionString)
        {
            _queueName = queueName;
            _degradedThreshold = degradedThreshold;
            _unhealthyThreshold = unhealthyThreshold;
        }

        public AzureServiceBusQueueMessageThresholdCountHealthCheck(string endpoint, string queueName, TokenCredential tokenCredential,  int degradedThreshold = 5, int unhealthyThreshold = 10) 
            : base(endpoint, tokenCredential)
        {
            _queueName = queueName;
            _degradedThreshold = degradedThreshold;
            _unhealthyThreshold = unhealthyThreshold;
        }

        protected override string ConnectionKey => $"{Prefix}_{_queueName}";

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ManagementClientConnections.TryGetValue(ConnectionKey, out var managementClient))
                {
                    managementClient = CreateManagementClient();

                    if (!ManagementClientConnections.TryAdd(ConnectionKey, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "No service bus administration client connection can't be added into dictionary.");
                    }
                }

                var properties = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken);
                if (properties.Value.ActiveMessageCount >= _unhealthyThreshold)
                    return HealthCheckResult.Unhealthy();

                if(properties.Value.ActiveMessageCount >= _degradedThreshold)
                    return HealthCheckResult.Degraded();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}