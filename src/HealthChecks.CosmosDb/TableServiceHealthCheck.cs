using Azure.Core;
using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb;

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
        _tableServiceClient = Guard.ThrowIfNull(tableServiceClient);
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: TableServiceClient.GetPropertiesAsync() cannot be used with only the role assignment
            // "Storage Table Data Contributor," so TableServiceClient.QueryAsync() and
            // TableClient.QueryAsync<T>() are used instead to probe service health.
            await _tableServiceClient
                .QueryAsync(filter: "false", cancellationToken: cancellationToken)
                .GetAsyncEnumerator(cancellationToken)
                .MoveNextAsync()
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_options.TableName))
            {
                var tableClient = _tableServiceClient.GetTableClient(_options.TableName);
                await tableClient
                    .QueryAsync<TableEntity>(filter: "false", cancellationToken: cancellationToken)
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync()
                    .ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
