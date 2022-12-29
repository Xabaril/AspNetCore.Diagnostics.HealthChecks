using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureBlobStorageHealthCheckOptions _options;

        public AzureBlobStorageHealthCheck(string connectionString, string? containerName = default, BlobClientOptions? clientOptions = default)
            : this(
                  ClientCache.GetOrAdd(connectionString, k => new BlobServiceClient(k, clientOptions)),
                  new AzureBlobStorageHealthCheckOptions { ContainerName = containerName })
        { }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? containerName = default, BlobClientOptions? clientOptions = default)
            : this(
                  ClientCache.GetOrAdd(blobServiceUri?.ToString()!, _ => new BlobServiceClient(blobServiceUri, credential, clientOptions)),
                  new AzureBlobStorageHealthCheckOptions { ContainerName = containerName })
        { }

        public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, AzureBlobStorageHealthCheckOptions options)
        {
            _blobServiceClient = Guard.ThrowIfNull(blobServiceClient);
            _options = Guard.ThrowIfNull(options);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Note: BlobServiceClient.GetPropertiesAsync() cannot be used with only the role assignment
                // "Storage Blob Data Contributor," so BlobServiceClient.GetBlobContainersAsync() is used instead to probe service health.
                // However, BlobContainerClient.GetPropertiesAsync() does have sufficient permissions.
                await _blobServiceClient
                    .GetBlobContainersAsync(cancellationToken: cancellationToken)
                    .AsPages(pageSizeHint: 1)
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync()
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(_options.ContainerName))
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
                    await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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
