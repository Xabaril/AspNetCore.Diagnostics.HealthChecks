using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb;

/// <summary>
/// Azure Tables health check.
/// </summary>
public sealed class AzureTableServiceHealthCheck : IHealthCheck
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly AzureTableServiceHealthCheckOptions _options;

    /// <summary>
    /// Creates new instance of Azure Tables health check.
    /// </summary>
    /// <param name="tableServiceClient">
    /// The <see cref="TableServiceClient"/> used to perform Azure Tables operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="options">Optional settings used by the health check.</param>
    public AzureTableServiceHealthCheck(TableServiceClient tableServiceClient, AzureTableServiceHealthCheckOptions? options)
    {
        _tableServiceClient = Guard.ThrowIfNull(tableServiceClient);
        _options = options ?? new();
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
