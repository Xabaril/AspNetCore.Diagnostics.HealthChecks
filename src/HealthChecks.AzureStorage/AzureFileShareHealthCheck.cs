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
            _shareServiceClient = shareServiceClient ?? throw new ArgumentNullException(nameof(shareServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _shareServiceClient.GetPropertiesAsync(cancellationToken);

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
