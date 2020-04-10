using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
                var queueServiceClient = new QueueServiceClient(_connectionString);
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
    }
}
