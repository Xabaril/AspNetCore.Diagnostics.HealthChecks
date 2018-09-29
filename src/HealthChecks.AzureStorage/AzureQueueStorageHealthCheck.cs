using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
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
        private readonly CloudStorageAccount _storageAccount;
        private readonly ILogger<AzureQueueStorageHealthCheck> _logger;

        public AzureQueueStorageHealthCheck(string connectionString, ILogger<AzureQueueStorageHealthCheck> logger = null)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureQueueStorageHealthCheck)} is checking the Azure Queue.");

                var blobClient = _storageAccount.CreateCloudQueueClient();
                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new QueueRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                _logger?.LogInformation($"The {nameof(AzureQueueStorageHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureQueueStorageHealthCheck)} check fail for {_storageAccount.QueueStorageUri} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
