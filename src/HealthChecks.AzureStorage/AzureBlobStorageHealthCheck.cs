using System.Collections.Concurrent;
using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly Func<BlobServiceClient> _clientFactory;
        private readonly string? _containerName;

        private static readonly ConcurrentDictionary<string, BlobServiceClient> _blobClientsHolder = new();

        public AzureBlobStorageHealthCheck(string connectionString, string? containerName = default, BlobClientOptions? clientOptions = null)
            : this(CreateClientFactory(connectionString, clientOptions), containerName)
        { }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string? containerName = default, BlobClientOptions? clientOptions = null)
            : this(CreateClientFactory(blobServiceUri, credential, clientOptions), containerName)
        { }

        public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, string? containerName = default)
            : this(CreateClientFactory(blobServiceClient), containerName)
        { }

        private AzureBlobStorageHealthCheck(Func<BlobServiceClient> clientFactory, string? containerName)
        {
            _clientFactory = clientFactory;
            _containerName = containerName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobServiceClient = _clientFactory();
                await foreach (var page in blobServiceClient.GetBlobContainersAsync(cancellationToken: cancellationToken).AsPages(pageSizeHint: 1))
                {
                    break;
                }

                if (!string.IsNullOrEmpty(_containerName))
                {
                    var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

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

        private static Func<BlobServiceClient> CreateClientFactory(string connectionString, BlobClientOptions? clientOptions = null)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return () => _blobClientsHolder.GetOrAdd(connectionString, s => new BlobServiceClient(s, clientOptions));
        }

        private static Func<BlobServiceClient> CreateClientFactory(Uri blobServiceUri, TokenCredential azureCredential, BlobClientOptions? clientOptions = null)
        {
            if (blobServiceUri == null)
            {
                throw new ArgumentNullException(nameof(blobServiceUri));
            }

            if (azureCredential == null)
            {
                throw new ArgumentNullException(nameof(azureCredential));
            }

            return () => _blobClientsHolder.GetOrAdd(
                blobServiceUri.ToString(),
                _ => new BlobServiceClient(blobServiceUri, azureCredential, clientOptions));
        }

        private static Func<BlobServiceClient> CreateClientFactory(BlobServiceClient blobServiceClient)
        {
            if (blobServiceClient == null)
            {
                throw new ArgumentNullException(nameof(blobServiceClient));
            }

            return () => blobServiceClient;
        }
    }
}
