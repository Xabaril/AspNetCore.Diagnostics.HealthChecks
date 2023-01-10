using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusSubscriptionHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly AzureServiceBusOptions _options;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private string? _connectionKey;

        public AzureServiceBusSubscriptionHealthCheck(string connectionString, string topicName, string subscriptionName, AzureServiceBusOptions options)
            : base(connectionString)
        {
            _options = options;
            _topicName = Guard.ThrowIfNull(topicName, true);
            _subscriptionName = Guard.ThrowIfNull(subscriptionName, true);
        }

        public AzureServiceBusSubscriptionHealthCheck(string endPoint, string topicName, string subscriptionName, TokenCredential tokenCredential, AzureServiceBusOptions options)
            : base(endPoint, tokenCredential)
        {
            _options = options;
            _topicName = Guard.ThrowIfNull(topicName, true);
            _subscriptionName = Guard.ThrowIfNull(subscriptionName, true);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_options.UsePeekMode)
                    await CheckWithReceiver();
                else
                    await CheckWithManagement();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }

            async Task CheckWithReceiver()
            {
                var client = ClientConnections.GetOrAdd(ConnectionKey, _ => CreateClient());
                var receiver = ServiceBusReceivers.GetOrAdd($"{nameof(AzureServiceBusSubscriptionHealthCheck)}_{ConnectionKey}", client.CreateReceiver(_topicName, _subscriptionName));
                _ = await receiver.PeekMessageAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            async Task CheckWithManagement()
            {
                var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());
                _ = await managementClient.GetSubscriptionRuntimePropertiesAsync(_topicName, _subscriptionName, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_topicName}_{_subscriptionName}";
    }
}
