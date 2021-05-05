using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    using Azure.Core;

    public class AzureServiceBusSubscriptionHealthCheck   : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _topicName;
        private readonly string _subscriptionName;

        public AzureServiceBusSubscriptionHealthCheck(string connectionString, string topicName, string subscriptionName) : base(connectionString)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            if (string.IsNullOrEmpty(subscriptionName))
            {
                throw new ArgumentNullException(nameof(subscriptionName));
            }

            _topicName = topicName;
            _subscriptionName = subscriptionName;
        }

        public AzureServiceBusSubscriptionHealthCheck(string endPoint, string topicName, string subscriptionName, TokenCredential tokenCredential) :base(endPoint,tokenCredential)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            if (string.IsNullOrEmpty(subscriptionName))
            {
                throw new ArgumentNullException(nameof(subscriptionName));
            }

            _topicName = topicName;
            _subscriptionName = subscriptionName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = ConnectionKey;
                if (!ManagementClientConnections.TryGetValue(connectionKey, out var managementClient))
                {
                    managementClient = CreateManagementClient();
                    if (!ManagementClientConnections.TryAdd(connectionKey, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description:
                            "New service bus administration client connection can't be added into dictionary.");
                    }
                }

                _ = await managementClient.GetSubscriptionRuntimePropertiesAsync(_topicName, _subscriptionName, cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        protected override string ConnectionKey => $"{Prefix}_{_topicName}_{_subscriptionName}";
    }
}
