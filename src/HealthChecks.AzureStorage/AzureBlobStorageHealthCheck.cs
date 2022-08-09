using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobStorageHealthCheckOptions _options;

        public AzureBlobStorageHealthCheck(string connectionString, string? containerName = default, BlobClientOptions? clientOptions = default)
            : this(
                  ClientCache<BlobServiceClient>.GetOrAdd(connectionString, k => new BlobServiceClient(k, clientOptions)),
                  new BlobStorageHealthCheckOptions { ContainerName = containerName })
        { }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? containerName = default, BlobClientOptions? clientOptions = default)
            : this(
                  ClientCache<BlobServiceClient>.GetOrAdd(blobServiceUri?.ToString()!, _ => new BlobServiceClient(blobServiceUri, credential, clientOptions)),
                  new BlobStorageHealthCheckOptions { ContainerName = containerName })
        { }

        public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, BlobStorageHealthCheckOptions options)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await _blobServiceClient.GetPropertiesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(_options.ContainerName))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
                await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
