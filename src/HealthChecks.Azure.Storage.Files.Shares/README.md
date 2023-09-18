## Azure File Storage Health Check

This health check verifies the ability to communicate with [Azure File Storage](https://azure.microsoft.com/en-us/products/storage/files/). It uses the provided [ShareServiceClient](https://learn.microsoft.com/dotnet/api/azure.storage.files.shares.shareserviceclient) to get first share or configured share properties.

### Defaults

By default, the `ShareServiceClient` instance is resolved from service provider. `AzureFileShareHealthCheckOptions` does not provide any specific share name, so the health check fetches just first share.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new ShareServiceClient(new Uri("azure-file-share-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureFileShare();
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide `ShareServiceClient` instance.
- `optionsFactory`: A factory method to provide `AzureFileShareHealthCheckOptions` instance. It allows to specify the share name.
- `healthCheckName`: The health check name. The default is `azure_file_share`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new ShareServiceClient(new Uri("azure-file-share-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureFileShare(
        optionsFactory: sp => new AzureFileShareHealthCheckOptions()
        {
            ShareName = "demo"
        });
}
```

### Breaking changes

In the prior releases, `AzureFileShareHealthCheck` was a part of `HealthChecks.AzureStorage` package. It had a dependency on not just `Azure.Storage.Files.Shares`, but also `Azure.Storage.Queues` and `Azure.Storage.Blobs`. The packages have been split to avoid bringing unnecessary dependencies. Moreover, `AzureFileShareHealthCheck` was letting the users specify how `ShareServiceClient` should be created (from raw connection string), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `ShareServiceClient`** (please see [#2040](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2040) for more details). Since Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/> and client instances can be expensive to create, it's recommended to register a singleton factory method for Azure SDK clients. So the clients are created only when needed and once per whole application lifetime.
