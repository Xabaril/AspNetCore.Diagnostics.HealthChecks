using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureFileShareHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _shareName;

        public AzureFileShareHealthCheck(string connectionString, string shareName = default)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _shareName = shareName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var shareClient = new ShareClient(_connectionString, _shareName);

                if (!await shareClient.ExistsAsync(cancellationToken))
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"File Share '{_shareName}' does not exists");
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
