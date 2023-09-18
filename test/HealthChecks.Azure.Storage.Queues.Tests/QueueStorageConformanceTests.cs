using Azure.Identity;
using Azure.Storage.Queues;

namespace HealthChecks.Azure.Storage.Queues.Tests;

public class QueueStorageConformanceTests : ConformanceTests<QueueServiceClient, AzureQueueStorageHealthCheck, AzureQueueStorageHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, QueueServiceClient>? clientFactory = null, Func<IServiceProvider, AzureQueueStorageHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureQueueStorage(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override QueueServiceClient CreateClientForNonExistingEndpoint()
    {
        QueueClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureQueueStorageHealthCheck CreateHealthCheck(QueueServiceClient client, AzureQueueStorageHealthCheckOptions? options)
        => new(client, options);

    protected override AzureQueueStorageHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
