using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly QueueStorageHealthCheckOptions _options;

        public AzureQueueStorageHealthCheck(string connectionString, string? queueName = default, QueueClientOptions? clientOptions = null)
            : this(
                  ClientCache<QueueServiceClient>.GetOrAdd(connectionString, k => new QueueServiceClient(k, clientOptions)),
                  Options.Create(new QueueStorageHealthCheckOptions { QueueName = queueName }))
        { }

        public AzureQueueStorageHealthCheck(Uri queueServiceUri, TokenCredential credential, string? queueName = default, QueueClientOptions? clientOptions = null)
            : this(
                  ClientCache<QueueServiceClient>.GetOrAdd(queueServiceUri?.ToString()!, _ => new QueueServiceClient(queueServiceUri, credential, clientOptions)),
                  Options.Create(new QueueStorageHealthCheckOptions { QueueName = queueName }))
        { }

        public AzureQueueStorageHealthCheck(QueueServiceClient queueServiceClient, IOptions<QueueStorageHealthCheckOptions> options)
        {
            _queueServiceClient = queueServiceClient ?? throw new ArgumentNullException(nameof(queueServiceClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var serviceProperties = await _queueServiceClient.GetPropertiesAsync(cancellationToken);

                if (!string.IsNullOrEmpty(_options.QueueName))
                {
                    var queueClient = _queueServiceClient.GetQueueClient(_options.QueueName);

                    if (!await queueClient.ExistsAsync(cancellationToken))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Queue '{queueClient.Name}' not exists");
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
