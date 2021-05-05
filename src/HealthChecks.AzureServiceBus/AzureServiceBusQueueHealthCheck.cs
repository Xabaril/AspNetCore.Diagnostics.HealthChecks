using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _queueName;

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName) : base(connectionString)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }
        }

        public AzureServiceBusQueueHealthCheck(string endPoint, string queueName, TokenCredential tokenCredential) :
            base(endPoint, tokenCredential)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            _queueName = queueName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
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

                await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }


        protected override string ConnectionKey => $"{Prefix}_{_queueName}";
    }
}
