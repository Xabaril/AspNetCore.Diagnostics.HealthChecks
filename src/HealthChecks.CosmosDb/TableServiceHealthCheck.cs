using Azure.Core;
using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb
{
    public class TableServiceHealthCheck : IHealthCheck
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableServiceHealthCheckOptions _options;

        public TableServiceHealthCheck(string connectionString, string? tableName)
            : this(
                  ClientCache.GetOrAdd(connectionString, k => new TableServiceClient(k)),
                  new TableServiceHealthCheckOptions { TableName = tableName })
        { }

        public TableServiceHealthCheck(Uri endpoint, TableSharedKeyCredential credentials, string? tableName)
            : this(
                  ClientCache.GetOrAdd(endpoint?.ToString()!, _ => new TableServiceClient(endpoint, credentials)),
                  new TableServiceHealthCheckOptions { TableName = tableName })
        { }

        public TableServiceHealthCheck(Uri endpoint, TokenCredential tokenCredential, string? tableName)
            : this(
                  ClientCache.GetOrAdd(endpoint?.ToString()!, _ => new TableServiceClient(endpoint, tokenCredential)),
                  new TableServiceHealthCheckOptions { TableName = tableName })
        { }

        public TableServiceHealthCheck(TableServiceClient tableServiceClient, TableServiceHealthCheckOptions options)
        {
            _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _tableServiceClient.GetPropertiesAsync(cancellationToken);

                if (!string.IsNullOrEmpty(_options.TableName))
                {
                    var tableClient = _tableServiceClient.GetTableClient(_options.TableName);
                    await tableClient.GetAccessPoliciesAsync(cancellationToken);
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
