## Azure File Storage Health Check

This health check verifies the ability to communicate with [Azure File Storage](https://azure.microsoft.com/en-us/products/storage/files/). It uses the provided [ShareServiceClient](https://learn.microsoft.com/dotnet/api/azure.storage.files.shares.shareserviceclient) to get first share or configured share properties.

### Defaults

By default, the `ShareServiceClient` instance is resolved from service provider. `AzureFileShareHealthCheckOptions` does not provide any specific share name, so the health check fetches just first share.

```csharp
public void Configure(IHealthChecksBuilder builder)
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
public void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new ShareServiceClient(new Uri("azure-file-share-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureFileShare(
        optionsFactory: sp => new AzureFileShareHealthCheckOptions()
        {
            ShareName = "demo"
        });
}
```

For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme)
