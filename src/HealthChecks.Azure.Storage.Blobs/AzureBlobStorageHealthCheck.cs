using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.Storage.Blobs;

/// <summary>
/// Azure Blob Storage health check.
/// </summary>
public sealed class AzureBlobStorageHealthCheck : IHealthCheck
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobStorageHealthCheckOptions _options;

    /// <summary>
    /// Creates new instance of Azure Blob Storage health check.
    /// </summary>
    /// <param name="blobServiceClient">
    /// The <see cref="BlobServiceClient"/> used to perform Azure Blob Storage operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="options">Optional settings used by the health check.</param>
    public AzureBlobStorageHealthCheck(BlobServiceClient blobServiceClient, AzureBlobStorageHealthCheckOptions? options = default)
    {
        _blobServiceClient = Guard.ThrowIfNull(blobServiceClient);
        _options = options ?? new AzureBlobStorageHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(_options.ContainerName))
            {
                // Note: PoLP (Principle of least privilege)
                // This can be used having at least the role assignment "Storage Blob Data Reader" at container level or at least "Storage Blob Data Reader" at storage account level.
                // See <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-app?tabs=dotnet#configure-permissions-for-access-to-blob-and-queue-data">Configure permissions for access to blob and queue data</see>
                var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
                await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Note: PoLP (Principle of least privilege)
                // This can be used having at least "Storage Blob Data Reader" at storage account level.
                // See <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-app?tabs=dotnet#configure-permissions-for-access-to-blob-and-queue-data">Configure permissions for access to blob and queue data</see>
                await _blobServiceClient
                    .GetBlobContainersAsync(cancellationToken: cancellationToken)
                    .AsPages(pageSizeHint: 1)
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync()
                    .ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
