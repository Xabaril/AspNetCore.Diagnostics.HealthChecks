using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureFileShareHealthCheck : IHealthCheck
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly AzureFileShareHealthCheckOptions _options;

        public AzureFileShareHealthCheck(string connectionString, string? shareName = default)
            : this(
                  ClientCache.GetOrAdd(connectionString, _ => new ShareServiceClient(connectionString)),
                  new AzureFileShareHealthCheckOptions { ShareName = shareName })
        { }

        public AzureFileShareHealthCheck(ShareServiceClient shareServiceClient, AzureFileShareHealthCheckOptions options)
        {
            _shareServiceClient = Guard.ThrowIfNull(shareServiceClient);
            _options = Guard.ThrowIfNull(options);
        }

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
                    .MoveNextAsync();

                if (!string.IsNullOrEmpty(_options.ShareName))
                {
                    var shareClient = _shareServiceClient.GetShareClient(_options.ShareName);
                    await shareClient.GetPropertiesAsync(cancellationToken);
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
