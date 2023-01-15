using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly AzureServiceBusQueueHealthCheckOptions _options;
        private string? _connectionKey;

        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_options.QueueName}";

        public AzureServiceBusQueueHealthCheck(AzureServiceBusQueueHealthCheckOptions options)
            : base(options)
        {
            Guard.ThrowIfNull(options.QueueName, true);

            _options = options;
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_options.UsePeekMode)
                    await CheckWithReceiver().ConfigureAwait(false);
                else
                    await CheckWithManagement().ConfigureAwait(false);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }

            Task CheckWithReceiver()
            {
                var client = ClientConnections.GetOrAdd(ConnectionKey, _ => CreateClient());
                var receiver = ServiceBusReceivers.GetOrAdd(
                    $"{nameof(AzureServiceBusQueueHealthCheck)}_{ConnectionKey}",
                    client.CreateReceiver(_options.QueueName));

                return receiver.PeekMessageAsync(cancellationToken: cancellationToken);
            }

            Task CheckWithManagement()
            {
                var managementClient = ManagementClientConnections.GetOrAdd(
                    ConnectionKey, _ => CreateManagementClient());

                return managementClient.GetQueueRuntimePropertiesAsync(_options.QueueName, cancellationToken);
            }
        }
    }
}
