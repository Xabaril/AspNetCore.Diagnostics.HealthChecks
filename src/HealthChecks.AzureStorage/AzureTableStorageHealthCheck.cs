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
        private readonly string _nameTable;
        public AzureTableStorageHealthCheck(string connectionString, string nameTable = default)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _connectionString = connectionString;
            _nameTable = nameTable;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_connectionString);
                var blobClient = storageAccount.CreateCloudTableClient();

                var serviceProperties = await blobClient.GetServicePropertiesAsync(
                    new TableRequestOptions(),
                    operationContext: null,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(_nameTable))
                {
                    var table = blobClient.GetTableReference(_nameTable);
                    if (!await table.ExistsAsync())
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Table '{_nameTable}' not exists");
                    }
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
