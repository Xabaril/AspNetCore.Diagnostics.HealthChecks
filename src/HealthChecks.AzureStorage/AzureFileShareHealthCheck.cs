using System.Collections.Concurrent;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureStorage
{
    public class AzureFileShareHealthCheck : IHealthCheck
    {
        private readonly Func<ShareClient> _clientFactory;

        private static readonly ConcurrentDictionary<string, ShareClient> _shareClientsHolder = new();

        public AzureFileShareHealthCheck(string connectionString, string? shareName = default)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _clientFactory = () => _shareClientsHolder.GetOrAdd($"{connectionString}{shareName}", _ => new ShareClient(connectionString, shareName));
        }

        public AzureFileShareHealthCheck(ShareClient shareClient)
        {
            if (shareClient == null)
            {
                throw new ArgumentNullException(nameof(shareClient));
            }

            _clientFactory = () => shareClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var shareClient = _clientFactory();

                if (!await shareClient.ExistsAsync(cancellationToken))
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"File Share '{shareClient.Name}' does not exist");
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
