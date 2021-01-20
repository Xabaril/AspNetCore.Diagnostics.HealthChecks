using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
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
        private readonly BlobClientOptions _clientOptions;

        private static readonly ConcurrentDictionary<string, BlobServiceClient> _blobClientsHolder
            = new ConcurrentDictionary<string, BlobServiceClient>();

        public AzureBlobStorageHealthCheck(string connectionString, string containerName = default, BlobClientOptions clientOptions = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _containerName = containerName;
            _clientOptions = clientOptions;
        }

        public AzureBlobStorageHealthCheck(Uri blobServiceUri, TokenCredential credential, string containerName = default, BlobClientOptions clientOptions = null)
        {
            _blobServiceUri = blobServiceUri ?? throw new ArgumentNullException(nameof(blobServiceUri));
            _azureCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            _containerName = containerName;
            _clientOptions = clientOptions;
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
            var serviceUri = _connectionString ?? _blobServiceUri.ToString();

            if (!_blobClientsHolder.TryGetValue(serviceUri, out BlobServiceClient client))
            {
                if (_connectionString != null)
                {
                    client = new BlobServiceClient(_connectionString, _clientOptions);
                }
                else
                {
                    client = new BlobServiceClient(_blobServiceUri, _azureCredential, _clientOptions);
                }

                _blobClientsHolder.TryAdd(serviceUri, client);
            }

            return client;
        }
    }
}
