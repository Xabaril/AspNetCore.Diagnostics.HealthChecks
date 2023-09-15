## Azure Blob Storage Health Check

This health check verifies the ability to communicate with [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs/). It uses the provided [BlobServiceClient](https://learn.microsoft.com/dotnet/api/azure.storage.blobs.blobserviceclient) to get first or configured blob container.

### Defaults

By default, the `BlobServiceClient` instance is resolved from service provider. `AzureBlobStorageHealthCheckOptions` does not provide any specific container name, so the health check fetches just first container.

```csharp
public void Configure(IHealthChecksBuilder builder)
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
public void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new BlobServiceClient(new Uri("azure-blob-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureBlobStorage(
        optionsFactory: sp => new AzureBlobStorageHealthCheckOptions()
        {
            ContainerName = "demo"
        });
}
```

For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme)
