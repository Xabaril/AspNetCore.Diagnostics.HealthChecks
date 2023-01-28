using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusTopicHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly AzureServiceBusTopicHealthCheckOptions _options;
        private string? _connectionKey;

        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_options.TopicName}";

        public AzureServiceBusTopicHealthCheck(AzureServiceBusTopicHealthCheckOptions options)
            : base(options)
        {
            Guard.ThrowIfNull(options.TopicName, true);

            _options = options;
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

                _ = await managementClient.GetTopicRuntimePropertiesAsync(_options.TopicName, cancellationToken)
                    .ConfigureAwait(false);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
