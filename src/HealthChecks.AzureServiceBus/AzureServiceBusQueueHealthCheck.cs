using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly AzureServiceBusOptions _options;
        private readonly string _queueName;
        private string? _connectionKey;

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName, AzureServiceBusOptions options)
            : base(connectionString)
        {
            _options = options;
            _queueName = Guard.ThrowIfNull(queueName, true);
        }

        public AzureServiceBusQueueHealthCheck(string endPoint, string queueName, TokenCredential tokenCredential, AzureServiceBusOptions options)
            : base(endPoint, tokenCredential)
        {
            _options = options;
            _queueName = Guard.ThrowIfNull(queueName, true);
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
                var receiver = ServiceBusReceivers.GetOrAdd($"{nameof(AzureServiceBusQueueHealthCheck)}_{ConnectionKey}", client.CreateReceiver(_queueName));
                _ = await receiver.PeekMessageAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            async Task CheckWithManagement()
            {
                var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());
                _ = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_queueName}";
    }
}
