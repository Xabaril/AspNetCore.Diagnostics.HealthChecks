using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck
        : IHealthCheck
    {
        private readonly TokenCredential _azureCredential;
        private readonly Uri _blobServiceUri;

        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageHealthCheck(string connectionString, string containerName = default)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _containerName = containerName;
        }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string containerName = default)
        {
            _blobServiceUri = blobServiceUri ?? throw new ArgumentNullException(nameof(blobServiceUri));
            _azureCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            _containerName = containerName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobServiceClient = GetBlobServiceClient();
                var serviceProperties = await blobServiceClient.GetPropertiesAsync(cancellationToken);

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

        private BlobServiceClient GetBlobServiceClient()
        {
            BlobServiceClient blobServiceClient;

            if (_blobServiceUri != null && _azureCredential != null)
            {
                blobServiceClient = new BlobServiceClient(_blobServiceUri, _azureCredential);
            }
            else
            {
                blobServiceClient = new BlobServiceClient(_connectionString);
            }

            return blobServiceClient;
        }
    }
}
