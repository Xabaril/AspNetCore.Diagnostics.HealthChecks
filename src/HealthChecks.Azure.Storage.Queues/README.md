## Azure Queue Storage Health Check

This health check verifies the ability to communicate with [Azure Queue Storage](https://azure.microsoft.com/en-us/products/storage/queues/). It uses the provided [QueueServiceClient](https://learn.microsoft.com/dotnet/api/azure.storage.queues.queueserviceclient) to get first queue or configured queues properties.

### Defaults

By default, the `QueueServiceClient` instance is resolved from service provider. `AzureQueueStorageHealthCheckOptions` does not provide any specific queue name, so the health check fetches just first queue.

```csharp
public void Configure(IHealthChecksBuilder builder)
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
public void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new QueueServiceClient(new Uri("azure-queue-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureQueueStorage(
        optionsFactory: sp => new AzureQueueStorageHealthCheckOptions()
        {
            QueueName = "demo"
        });
}
```

For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme)
