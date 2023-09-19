using Azure.Storage.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.Storage.Queues;

/// <summary>
/// Azure Queue Storage health check.
/// </summary>
public sealed class AzureQueueStorageHealthCheck : IHealthCheck
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly AzureQueueStorageHealthCheckOptions _options;

    /// <summary>
    /// Creates new instance of Azure Queue Storage health check.
    /// </summary>
    /// <param name="queueServiceClient">
    /// The <see cref="QueueServiceClient"/> used to perform Azure Queue Storage operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="options">Optional settings used by the health check.</param>
    public AzureQueueStorageHealthCheck(QueueServiceClient queueServiceClient, AzureQueueStorageHealthCheckOptions? options = default)
    {
        _queueServiceClient = Guard.ThrowIfNull(queueServiceClient);
        _options = options ?? new();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: QueueServiceClient.GetPropertiesAsync() cannot be used with only the role assignment
            // "Storage Queue Data Contributor," so QueueServiceClient.GetQueuesAsync() is used instead to probe service health.
            // However, QueueClient.GetPropertiesAsync() does have sufficient permissions.
            await _queueServiceClient
                .GetQueuesAsync(cancellationToken: cancellationToken)
                .AsPages(pageSizeHint: 1)
                .GetAsyncEnumerator(cancellationToken)
                .MoveNextAsync()
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_options.QueueName))
            {
                var queueClient = _queueServiceClient.GetQueueClient(_options.QueueName);
                await queueClient.GetPropertiesAsync(cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
