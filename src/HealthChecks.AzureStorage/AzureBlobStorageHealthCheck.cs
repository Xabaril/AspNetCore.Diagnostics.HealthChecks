using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        private readonly string _connectionString;
        public AzureBlobStorageHealthCheck(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _connectionString = connectionString;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new BlobRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
