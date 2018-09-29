using BeatPulse.Core;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatPulse.AzureStorage
{
    public class AzureTableStorageLiveness : IBeatPulseLiveness
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly ILogger<AzureTableStorageLiveness> _logger;

        public AzureTableStorageLiveness(string connectionString,ILogger<AzureTableStorageLiveness> logger = null)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _logger = logger;
        }

        public async Task<LivenessResult> IsHealthy(LivenessExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureTableStorageLiveness)} is checking the Azure Table.");

                var blobClient = _storageAccount.CreateCloudTableClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new TableRequestOptions(), 
                    operationContext:null,
                    cancellationToken:cancellationToken);

                _logger?.LogInformation($"The {nameof(AzureTableStorageLiveness)} check success.");

                return LivenessResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureTableStorageLiveness)} check fail for {_storageAccount.TableStorageUri} with the exception {ex.ToString()}.");

                return LivenessResult.UnHealthy(ex);
            }
        }
    }
}
