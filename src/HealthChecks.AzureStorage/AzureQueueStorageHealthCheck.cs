using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck 
        : IHealthCheck
    {
        private readonly CloudStorageAccount _storageAccount;
        public AzureQueueStorageHealthCheck(string connectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = _storageAccount.CreateCloudQueueClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new QueueRequestOptions(),
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
