using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusTopicHealthCheck
        : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _topicName;
        private string? _connectionKey;

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName)
            : base(connectionString)
        {
            _topicName = Guard.ThrowIfNull(topicName, true);
        }

        public AzureServiceBusTopicHealthCheck(string endpoint, string topicName, TokenCredential tokenCredential)
            : base(endpoint, tokenCredential)
        {
            _topicName = Guard.ThrowIfNull(topicName, true);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());
                _ = await managementClient.GetTopicRuntimePropertiesAsync(_topicName, cancellationToken).ConfigureAwait(false);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{_topicName}";
    }
}
