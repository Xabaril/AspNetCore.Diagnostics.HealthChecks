using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        private readonly TokenCredential _azureCredential;
        private readonly Uri _queueServiceUri;

        private static readonly ConcurrentDictionary<string, QueueServiceClient> _queueClientsHolder
           = new ConcurrentDictionary<string, QueueServiceClient>();

        public AzureQueueStorageHealthCheck(string connectionString, string queueName = default)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName;
        }

        public AzureQueueStorageHealthCheck(Uri queueServiceUri, TokenCredential credential, string queueName = default)
        {
            _queueServiceUri = queueServiceUri ?? throw new ArgumentNullException(nameof(queueServiceUri));
            _azureCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            _queueName = queueName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var queueServiceClient = GetQueueServiceClient();
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

        private QueueServiceClient GetQueueServiceClient()
        {
            var serviceUri = _connectionString ?? _queueServiceUri.ToString();

            if (!_queueClientsHolder.TryGetValue(serviceUri, out QueueServiceClient client))
            {
                if (_connectionString != null)
                {
                    client = new QueueServiceClient(_connectionString);
                }
                else
                {
                    client = new QueueServiceClient(_queueServiceUri, _azureCredential);
                }

                _queueClientsHolder.TryAdd(serviceUri, client);
            }

            return client;
        }
    }
}
