using System.Collections.Concurrent;
using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly Func<QueueServiceClient> _clientFactory;
        private readonly string? _queueName;

        private static readonly ConcurrentDictionary<string, QueueServiceClient> _queueClientsHolder = new();

        public AzureQueueStorageHealthCheck(string connectionString, string? queueName = default, QueueClientOptions? clientOptions = null)
            : this(CreateClientFactory(connectionString, clientOptions), queueName)
        { }

        public AzureQueueStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? queueName = default, QueueClientOptions? clientOptions = null)
            : this(CreateClientFactory(blobServiceUri, credential, clientOptions), queueName)
        { }

        public AzureQueueStorageHealthCheck(QueueServiceClient blobServiceClient, string? queueName = default)
            : this(CreateClientFactory(blobServiceClient), queueName)
        { }

        private AzureQueueStorageHealthCheck(Func<QueueServiceClient> clientFactory, string? queueName)
        {
            _clientFactory = clientFactory;
            _queueName = queueName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var queueServiceClient = _clientFactory();
                var serviceProperties = await queueServiceClient.GetPropertiesAsync(cancellationToken);

                if (!string.IsNullOrEmpty(_queueName))
                {
                    var queueClient = queueServiceClient.GetQueueClient(_queueName);

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

        private static Func<QueueServiceClient> CreateClientFactory(string connectionString, QueueClientOptions? clientOptions = null)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return () => _queueClientsHolder.GetOrAdd(connectionString, s => new QueueServiceClient(s, clientOptions));
        }

        private static Func<QueueServiceClient> CreateClientFactory(Uri blobServiceUri, TokenCredential azureCredential, QueueClientOptions? clientOptions = null)
        {
            if (blobServiceUri == null)
            {
                throw new ArgumentNullException(nameof(blobServiceUri));
            }

            if (azureCredential == null)
            {
                throw new ArgumentNullException(nameof(azureCredential));
            }

            return () => _queueClientsHolder.GetOrAdd(
                blobServiceUri.ToString(),
                _ => new QueueServiceClient(blobServiceUri, azureCredential, clientOptions));
        }

        private static Func<QueueServiceClient> CreateClientFactory(QueueServiceClient blobServiceClient)
        {
            if (blobServiceClient == null)
            {
                throw new ArgumentNullException(nameof(blobServiceClient));
            }

            return () => blobServiceClient;
        }
    }
}
