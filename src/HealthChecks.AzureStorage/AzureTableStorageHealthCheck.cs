using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureTableStorageHealthCheck
        : IHealthCheck
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly ILogger<AzureTableStorageHealthCheck> _logger;

        public AzureTableStorageHealthCheck(string connectionString,ILogger<AzureTableStorageHealthCheck> logger = null)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureTableStorageHealthCheck)} is checking the Azure Table.");

                var blobClient = _storageAccount.CreateCloudTableClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new TableRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                _logger?.LogInformation($"The {nameof(AzureTableStorageHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureTableStorageHealthCheck)} check fail for {_storageAccount.TableStorageUri} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
