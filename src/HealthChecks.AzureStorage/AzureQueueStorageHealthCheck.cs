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
        private readonly string _connectionString;
        private readonly TimeSpan _timeout;

        public AzureQueueStorageHealthCheck(string connectionString, TimeSpan timeout)
        {
            _connectionString = connectionString;
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(_connectionString);
                    var blobClient = storageAccount.CreateCloudQueueClient();

                    var serviceProperties = await blobClient.GetServicePropertiesAsync(
                        new QueueRequestOptions(),
                        operationContext: null,
                        cancellationToken: timeoutCancellationTokenSource.Token);

                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }

                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}
