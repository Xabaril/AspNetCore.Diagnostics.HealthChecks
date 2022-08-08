using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureFileShareHealthCheck : IHealthCheck
    {
        private readonly ShareClient _shareClient;

        public AzureFileShareHealthCheck(string connectionString, string? shareName = default)
            : this(ClientCache<ShareClient>.GetOrAdd($"{connectionString}{shareName}", _ => new ShareClient(connectionString, shareName)))
        { }

        public AzureFileShareHealthCheck(ShareClient shareClient)
        {
            _shareClient = shareClient ?? throw new ArgumentNullException(nameof(shareClient));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await _shareClient.ExistsAsync(cancellationToken))
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"File Share '{_shareClient.Name}' does not exist");
                };

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
