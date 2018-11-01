using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public AzureTableStorageHealthCheck(string connectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = _storageAccount.CreateCloudTableClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new TableRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
