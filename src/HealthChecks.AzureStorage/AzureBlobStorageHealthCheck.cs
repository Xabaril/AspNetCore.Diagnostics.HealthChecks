using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobStorageHealthCheckOptions _options;

        public AzureBlobStorageHealthCheck(string connectionString, string? containerName = default, BlobClientOptions? clientOptions = null)
            : this(
                  ClientCache<BlobServiceClient>.GetOrAdd(connectionString, k => new BlobServiceClient(k, clientOptions)),
                  Options.Create(new BlobStorageHealthCheckOptions { ContainerName = containerName }))
        { }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? containerName = default, BlobClientOptions? clientOptions = null)
            : this(
                  ClientCache<BlobServiceClient>.GetOrAdd(blobServiceUri?.ToString()!, _ => new BlobServiceClient(blobServiceUri, credential, clientOptions)),
                  Options.Create(new BlobStorageHealthCheckOptions { ContainerName = containerName }))
        { }

        public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, IOptions<BlobStorageHealthCheckOptions> options)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await foreach (var page in _blobServiceClient.GetBlobContainersAsync(cancellationToken: cancellationToken).AsPages(pageSizeHint: 1))
                {
                    break;
                }

                if (!string.IsNullOrEmpty(_options.ContainerName))
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

                    if (!await containerClient.ExistsAsync(cancellationToken))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Container '{containerClient.Name}' not exists");
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
