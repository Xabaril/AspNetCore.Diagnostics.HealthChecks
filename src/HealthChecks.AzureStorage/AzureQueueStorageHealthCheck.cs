using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly AzureQueueStorageHealthCheckOptions _options;

        public AzureQueueStorageHealthCheck(string connectionString, string? queueName = default)
            : this(
                  ClientCache<QueueServiceClient>.GetOrAdd(connectionString, k => new QueueServiceClient(k)),
                  new AzureQueueStorageHealthCheckOptions { QueueName = queueName })
        { }

        public AzureQueueStorageHealthCheck(Uri queueServiceUri, TokenCredential credential, string? queueName = default)
            : this(
                  ClientCache<QueueServiceClient>.GetOrAdd(queueServiceUri?.ToString()!, _ => new QueueServiceClient(queueServiceUri, credential)),
                  new AzureQueueStorageHealthCheckOptions { QueueName = queueName })
        { }

        public AzureQueueStorageHealthCheck(QueueServiceClient queueServiceClient, AzureQueueStorageHealthCheckOptions options)
        {
            _queueServiceClient = queueServiceClient ?? throw new ArgumentNullException(nameof(queueServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await _queueServiceClient.GetPropertiesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(_options.QueueName))
            {
                var queueClient = _queueServiceClient.GetQueueClient(_options.QueueName);
                await queueClient.GetPropertiesAsync(cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
