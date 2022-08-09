using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly IOptionsSnapshot<QueueStorageHealthCheckOptions> _options;

        public AzureQueueStorageHealthCheck(string connectionString, string? queueName = default)
            : this(
                  ClientCache<QueueServiceClient>.GetOrAdd(connectionString, k => new QueueServiceClient(k)),
                  new SimpleSnapshot<QueueStorageHealthCheckOptions>(new QueueStorageHealthCheckOptions { QueueName = queueName }))
        { }

        public AzureQueueStorageHealthCheck(Uri queueServiceUri, TokenCredential credential, string? queueName = default)
            : this(
                  ClientCache<QueueServiceClient>.GetOrAdd(queueServiceUri?.ToString()!, _ => new QueueServiceClient(queueServiceUri, credential)),
                  new SimpleSnapshot<QueueStorageHealthCheckOptions>(new QueueStorageHealthCheckOptions { QueueName = queueName }))
        { }

        public AzureQueueStorageHealthCheck(QueueServiceClient queueServiceClient, IOptionsSnapshot<QueueStorageHealthCheckOptions> options)
        {
            _queueServiceClient = queueServiceClient ?? throw new ArgumentNullException(nameof(queueServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var options = _options.Get(context.Registration.Name);

            await _queueServiceClient.GetPropertiesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(options.QueueName))
            {
                var queueClient = _queueServiceClient.GetQueueClient(options.QueueName);
                await queueClient.GetPropertiesAsync(cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
