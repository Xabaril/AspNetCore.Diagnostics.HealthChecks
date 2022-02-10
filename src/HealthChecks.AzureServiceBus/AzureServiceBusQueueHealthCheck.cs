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
        private string _connectionKey;

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName) : base(connectionString)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            _queueName = queueName;
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
                var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());
                _ = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }


        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_queueName}";
    }
}
