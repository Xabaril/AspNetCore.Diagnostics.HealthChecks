using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureFileShareHealthCheck : IHealthCheck
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly FileShareHealthCheckOptions _options;

        public AzureFileShareHealthCheck(string connectionString, string? shareName = default)
            : this(
                  ClientCache<ShareServiceClient>.GetOrAdd(connectionString, _ => new ShareServiceClient(connectionString)),
                  new FileShareHealthCheckOptions { ShareName = shareName })
        { }

        public AzureFileShareHealthCheck(ShareServiceClient shareServiceClient, FileShareHealthCheckOptions options)
        {
            _shareServiceClient = shareServiceClient ?? throw new ArgumentNullException(nameof(shareServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await _shareServiceClient.GetPropertiesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(_options.ShareName))
            {
                var containerClient = _shareServiceClient.GetShareClient(_options.ShareName);
                await containerClient.GetPropertiesAsync(cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
