## Azure Queue Storage Health Check

This health check verifies the ability to communicate with [Azure Queue Storage](https://azure.microsoft.com/en-us/products/storage/queues/). It uses the provided [QueueServiceClient](https://learn.microsoft.com/dotnet/api/azure.storage.queues.queueserviceclient) to get first queue or configured queues properties.

### Defaults

By default, the `QueueServiceClient` instance is resolved from service provider. `AzureQueueStorageHealthCheckOptions` does not provide any specific queue name, so the health check fetches just first queue.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new QueueServiceClient(new Uri("azure-queue-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureQueueStorage();
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide `QueueServiceClient` instance.
- `optionsFactory`: A factory method to provide `AzureQueueStorageHealthCheckOptions` instance. It allows to specify the queue name.
- `healthCheckName`: The health check name. The default is `azure_queue_storage`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new QueueServiceClient(new Uri("azure-queue-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureQueueStorage(
        optionsFactory: sp => new AzureQueueStorageHealthCheckOptions()
        {
            QueueName = "demo"
        });
}
```

### Breaking changes

In the prior releases, `AzureQueueStorageHealthCheck` was a part of `HealthChecks.AzureStorage` package. It had a dependency on not just `Azure.Storage.Queues`, but also `Azure.Storage.Files.Shares` and `Azure.Storage.Blobs`. The packages have been split to avoid bringing unnecessary dependencies. Moreover, `AzureQueueStorageHealthCheck` was letting the users specify how `QueueServiceClient` should be created (from raw connection string or an endpoint with managed identity credentials), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `ShareServiceClient`** (please see [#2040](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2040) for more details). Since Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/> and client instances can be expensive to create, it's recommended to register a singleton factory method for Azure SDK clients. So the clients are created only when needed and once per whole application lifetime.

