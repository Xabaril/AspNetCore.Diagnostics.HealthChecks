using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage;

/// <summary>
/// Azure Files health check.
/// </summary>
public sealed class AzureFileShareHealthCheck : IHealthCheck
{
    private readonly ShareServiceClient _shareServiceClient;
    private readonly AzureFileShareHealthCheckOptions _options;

    public AzureFileShareHealthCheck(ShareServiceClient shareServiceClient, AzureFileShareHealthCheckOptions? options)
    {
        _shareServiceClient = Guard.ThrowIfNull(shareServiceClient);
        _options = options ?? new();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: ShareServiceClient does not support TokenCredentials as of writing, so only SAS tokens and
            // Account keys may be used to authenticate. However, like the health checks for Azure Blob Storage and
            // Azure Queue Storage, the AzureFileShareHealthCheck similarly enumerates the shares to probe service health.
            await _shareServiceClient
                .GetSharesAsync(cancellationToken: cancellationToken)
                .AsPages(pageSizeHint: 1)
                .GetAsyncEnumerator(cancellationToken)
                .MoveNextAsync()
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_options.ShareName))
            {
                var shareClient = _shareServiceClient.GetShareClient(_options.ShareName);
                await shareClient.GetPropertiesAsync(cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
