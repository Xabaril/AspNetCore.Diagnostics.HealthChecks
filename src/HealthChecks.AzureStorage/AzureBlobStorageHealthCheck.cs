using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureBlobStorageHealthCheck(string connectionString, string? containerName = default, BlobClientOptions? clientOptions = null)
            : this(ClientCache<BlobServiceClient>.GetOrAdd(connectionString, k => new BlobServiceClient(k, clientOptions)), containerName)
        { }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? containerName = default, BlobClientOptions? clientOptions = null)
            : this(ClientCache<BlobServiceClient>.GetOrAdd(blobServiceUri?.ToString()!, _ => new BlobServiceClient(blobServiceUri, credential, clientOptions)), containerName)
        { }

        public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, string? containerName = default)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await foreach (var page in _blobServiceClient.GetBlobContainersAsync(cancellationToken: cancellationToken).AsPages(pageSizeHint: 1))
                {
                    break;
                }

                if (!string.IsNullOrEmpty(_containerName))
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                    if (!await containerClient.ExistsAsync(cancellationToken))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Container '{_containerName}' not exists");
                    }

                    await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
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
