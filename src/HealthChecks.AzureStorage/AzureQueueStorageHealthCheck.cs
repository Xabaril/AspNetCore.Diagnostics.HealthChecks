using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly string _queueName;

        public AzureQueueStorageHealthCheck(string connectionString, string? queueName = default, QueueClientOptions? clientOptions = null)
            : this(ClientCache<QueueServiceClient>.GetOrAdd(connectionString, k => new QueueServiceClient(k, clientOptions)), queueName)
        { }

        public AzureQueueStorageHealthCheck(Uri queueServiceUri, TokenCredential credential, string? queueName = default, QueueClientOptions? clientOptions = null)
            : this(ClientCache<QueueServiceClient>.GetOrAdd(queueServiceUri?.ToString()!, _ => new QueueServiceClient(queueServiceUri, credential, clientOptions)), queueName)
        { }

        public AzureQueueStorageHealthCheck(QueueServiceClient queueServiceClient, string? queueName = default)
        {
            _queueServiceClient = queueServiceClient ?? throw new ArgumentNullException(nameof(queueServiceClient));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var serviceProperties = await _queueServiceClient.GetPropertiesAsync(cancellationToken);

                if (!string.IsNullOrEmpty(_queueName))
                {
                    var queueClient = _queueServiceClient.GetQueueClient(_queueName);

                    if (!await queueClient.ExistsAsync(cancellationToken))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Queue '{_queueName}' not exists");
                    }

                    await queueClient.GetPropertiesAsync(cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
