using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck 
        : IHealthCheck
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly ILogger<AzureBlobStorageHealthCheck> _logger;

        public AzureBlobStorageHealthCheck(string connectionString,ILogger<AzureBlobStorageHealthCheck> logger = null)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureBlobStorageHealthCheck)} is checking the Azure Blob.");

                var blobClient = _storageAccount.CreateCloudBlobClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new BlobRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                _logger?.LogInformation($"The {nameof(AzureBlobStorageHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureBlobStorageHealthCheck)} check fail for {_storageAccount.BlobStorageUri} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
