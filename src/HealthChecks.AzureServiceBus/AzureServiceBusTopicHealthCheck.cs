using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusTopicHealthCheck
        : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _topicName;

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName) : base(connectionString)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            _topicName = topicName;
        }

        public AzureServiceBusTopicHealthCheck(string endpoint, string topicName, TokenCredential tokenCredential) : base(endpoint, tokenCredential)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            _topicName = topicName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());
                _ = await managementClient.GetTopicRuntimePropertiesAsync(_topicName, cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        protected override string ConnectionKey => $"{Prefix}_{_topicName}";
    }
}
