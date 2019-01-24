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
        private readonly string _connectionString;
        private readonly TimeSpan _timeout;

        public AzureTableStorageHealthCheck(string connectionString, TimeSpan timeout)
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
                    var blobClient = storageAccount.CreateCloudTableClient();

                    var serviceProperties = await blobClient.GetServicePropertiesAsync(
                        new TableRequestOptions(),
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
