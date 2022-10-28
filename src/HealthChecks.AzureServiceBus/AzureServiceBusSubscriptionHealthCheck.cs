using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusSubscriptionHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private string? _connectionKey;

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

        public AzureServiceBusSubscriptionHealthCheck(string endPoint, string topicName, string subscriptionName, TokenCredential tokenCredential) : base(endPoint, tokenCredential)
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
                var client = ClientConnections.GetOrAdd(ConnectionKey, _ => CreateClient());
                var receiver = ServiceBusReceivers
                    .GetOrAdd($"{nameof(AzureServiceBusSubscriptionHealthCheck)}_{ConnectionKey}", client.CreateReceiver(_topicName, _subscriptionName));

                _ = await receiver.PeekMessageAsync(cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_topicName}_{_subscriptionName}";
    }
}
