## Azure Blob Storage Health Check

This health check verifies the ability to communicate with [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs/). It uses the provided [BlobServiceClient](https://learn.microsoft.com/dotnet/api/azure.storage.blobs.blobserviceclient) to get first or configured blob container.

### Defaults

By default, the `BlobServiceClient` instance is resolved from service provider. `AzureBlobStorageHealthCheckOptions` does not provide any specific container name, so the health check fetches just first container.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new BlobServiceClient(new Uri("azure-blob-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureBlobStorage();
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide `BlobServiceClient` instance.
- `optionsFactory`: A factory method to provide `AzureBlobStorageHealthCheckOptions` instance. It allows to specify the container name.
- `healthCheckName`: The health check name. The default is `azure_blob_storage`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new BlobServiceClient(new Uri("azure-blob-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureBlobStorage(
        optionsFactory: sp => new AzureBlobStorageHealthCheckOptions()
        {
            ContainerName = "demo"
        });
}
```

### Breaking changes

In the prior releases, `AzureBlobStorageHealthCheck` was a part of `HealthChecks.AzureStorage` package. It had a dependency on not just `Azure.Storage.Blobs`, but also `Azure.Storage.Queues` and `Azure.Storage.Files.Shares`. The packages have been split to avoid bringing unnecessary dependencies. Moreover, `AzureBlobStorageHealthCheck` was letting the users specify how `BlobServiceClient` should be created (from raw connection string or from endpoint uri and managed identity credentials), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `BlobServiceClient`** (please see [#2040](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2040) for more details). Since Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/> and client instances can be expensive to create, it's recommended to register a singleton factory method for Azure SDK clients. So the clients are created only when needed and once per whole application lifetime.

