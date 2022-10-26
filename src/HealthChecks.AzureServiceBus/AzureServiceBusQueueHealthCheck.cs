using Azure.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck, IHealthCheck
    {
        private readonly string _queueName;
        private string? _connectionKey;

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
                var client = ClientConnections.GetOrAdd(ConnectionKey, _ => CreateClient());
                var receiver = client.CreateReceiver(_queueName);
                _ = await receiver.PeekMessageAsync(cancellationToken: cancellationToken);
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
