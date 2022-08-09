using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptionsSnapshot<BlobStorageHealthCheckOptions> _options;

        public AzureBlobStorageHealthCheck(string connectionString, string? containerName = default, BlobClientOptions? clientOptions = default)
            : this(
                  ClientCache<BlobServiceClient>.GetOrAdd(connectionString, k => new BlobServiceClient(k, clientOptions)),
                  new SimpleSnapshot<BlobStorageHealthCheckOptions>(new BlobStorageHealthCheckOptions { ContainerName = containerName }))
        { }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? containerName = default, BlobClientOptions? clientOptions = default)
            : this(
                  ClientCache<BlobServiceClient>.GetOrAdd(blobServiceUri?.ToString()!, _ => new BlobServiceClient(blobServiceUri, credential, clientOptions)),
                  new SimpleSnapshot<BlobStorageHealthCheckOptions>(new BlobStorageHealthCheckOptions { ContainerName = containerName }))
        { }

        public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, IOptionsSnapshot<BlobStorageHealthCheckOptions> options)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var options = _options.Get(context.Registration.Name);

            await _blobServiceClient.GetPropertiesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(options.ContainerName))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(options.ContainerName);
                await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
