using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.AzureStorage
{
    public class AzureFileShareHealthCheck : IHealthCheck
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly IOptionsSnapshot<FileShareHealthCheckOptions> _options;

        public AzureFileShareHealthCheck(string connectionString, string? shareName = default)
            : this(
                  ClientCache<ShareServiceClient>.GetOrAdd(connectionString, _ => new ShareServiceClient(connectionString)),
                  new SimpleSnapshot<FileShareHealthCheckOptions>(new FileShareHealthCheckOptions { ShareName = shareName }))
        { }

        public AzureFileShareHealthCheck(ShareServiceClient shareServiceClient, IOptionsSnapshot<FileShareHealthCheckOptions> options)
        {
            _shareServiceClient = shareServiceClient ?? throw new ArgumentNullException(nameof(shareServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var options = _options.Get(context.Registration.Name);

            await _shareServiceClient.GetPropertiesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(options.ShareName))
            {
                var containerClient = _shareServiceClient.GetShareClient(options.ShareName);
                await containerClient.GetPropertiesAsync(cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
