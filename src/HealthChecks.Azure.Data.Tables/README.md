## Azure Tables Health Check

This health check verifies the ability to communicate with [Azure Tables](https://azure.microsoft.com/en-us/products/storage/tables/). It uses the provided [TableServiceClient](https://learn.microsoft.com/dotnet/api/azure.data.tables.tableserviceclient).

### Defaults

By default, the `TableServiceClient` instance is resolved from service provider. `AzureTableServiceHealthCheckOptions` does not provide any specific container name, so the health check fetches just first container.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new TableServiceClient(new Uri("azure-table-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureTable();
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide `TableServiceClient` instance.
- `optionsFactory`: A factory method to provide `AzureTableServiceHealthCheckOptions` instance. It allows to specify the table name.
- `healthCheckName`: The health check name. The default is `azure_tables`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new TableServiceClient(new Uri("azure-table-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureTable(
        optionsFactory: sp => new AzureTableServiceHealthCheckOptions()
        {
            TableName = "demo"
        });
}
```

### Breaking changes

In the prior releases, `TableServiceHealthCheck` was a part of `HealthChecks.CosmosDb` package. It had a dependency on not just `Azure.Data.Tables`, but also `Microsoft.Azure.Cosmos`. The packages have been split to avoid bringing unnecessary dependencies. Moreover, `TableServiceHealthCheck` was letting the users specify how `TableServiceClient` should be created (from raw connection string or from endpoint and managed identity credentials), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `TableServiceClient`** (please see [#2040](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2040) for more details). Since Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/> and client instances can be expensive to create, it's recommended to register a singleton factory method for Azure SDK clients. So the clients are created only when needed and once per whole application lifetime.

