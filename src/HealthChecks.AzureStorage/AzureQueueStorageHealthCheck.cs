using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck
        : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        public AzureQueueStorageHealthCheck(string connectionString, string queueName = default)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_connectionString);
                var blobClient = storageAccount.CreateCloudQueueClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new QueueRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(_queueName))
                {
                    var queue = blobClient.GetQueueReference(_queueName);
                    if (!await queue.ExistsAsync())
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Queue '{_queueName}' not exists");
                    }
                    await queue.FetchAttributesAsync();
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
