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
