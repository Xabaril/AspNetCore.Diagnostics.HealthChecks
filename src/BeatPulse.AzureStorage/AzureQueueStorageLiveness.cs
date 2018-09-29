using BeatPulse.Core;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatPulse.AzureStorage
{
    public class AzureQueueStorageLiveness : IBeatPulseLiveness
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly ILogger<AzureQueueStorageLiveness> _logger;

        public AzureQueueStorageLiveness(string connectionString, ILogger<AzureQueueStorageLiveness> logger = null)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _logger = logger;
        }

        public async Task<LivenessResult> IsHealthy(LivenessExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureQueueStorageLiveness)} is checking the Azure Queue.");

                var blobClient = _storageAccount.CreateCloudQueueClient();
                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new QueueRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                _logger?.LogInformation($"The {nameof(AzureQueueStorageLiveness)} check success.");

                return LivenessResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureQueueStorageLiveness)} check fail for {_storageAccount.QueueStorageUri} with the exception {ex.ToString()}.");

                return LivenessResult.UnHealthy(ex);
            }
        }
    }
}
