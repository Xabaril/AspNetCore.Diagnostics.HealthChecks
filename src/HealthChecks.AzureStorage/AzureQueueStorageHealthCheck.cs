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
                  ClientCache.GetOrAdd(connectionString, k => new QueueServiceClient(k)),
                  new AzureQueueStorageHealthCheckOptions { QueueName = queueName })
        { }

        public AzureQueueStorageHealthCheck(Uri queueServiceUri, TokenCredential credential, string? queueName = default)
            : this(
                  ClientCache.GetOrAdd(queueServiceUri?.ToString()!, _ => new QueueServiceClient(queueServiceUri, credential)),
                  new AzureQueueStorageHealthCheckOptions { QueueName = queueName })
        { }

        public AzureQueueStorageHealthCheck(QueueServiceClient queueServiceClient, AzureQueueStorageHealthCheckOptions options)
        {
            _queueServiceClient = Guard.ThrowIfNull(queueServiceClient);
            _options = Guard.ThrowIfNull(options);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Note: QueueServiceClient.GetPropertiesAsync() cannot be used with only the role assignment
                // "Storage Queue Data Contributor," so QueueServiceClient.GetQueuesAsync() is used instead to probe service health.
                // However, QueueClient.GetPropertiesAsync() does have sufficient permissions.
                await _queueServiceClient
                    .GetQueuesAsync(cancellationToken: cancellationToken)
                    .AsPages(pageSizeHint: 1)
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync();

                if (!string.IsNullOrEmpty(_options.QueueName))
                {
                    var queueClient = _queueServiceClient.GetQueueClient(_options.QueueName);
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
