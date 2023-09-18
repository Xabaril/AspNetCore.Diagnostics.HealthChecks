## Azure Tables Health Check

This health check verifies the ability to communicate with [Azure Cosmos DB](https://azure.microsoft.com/en-us/products/cosmos-db/). It uses the provided [CosmosClient](https://learn.microsoft.com/dotnet/api/microsoft.azure.cosmos.cosmosclient).

### Defaults

By default, the `CosmosClient` instance is resolved from service provider. `AzureCosmosDbHealthCheckOptions` does not provide any specific containers or database ids, the health check just calls [CosmosClient.ReadAccountAsync](https://learn.microsoft.com/dotnet/api/microsoft.azure.cosmos.cosmosclient.readaccountasync).

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new CosmosClient(
        "endpoint-from-portal",
        new DefaultAzureCredential(),
        new CosmosClientOptions()
        {
            ApplicationRegion = Regions.EastUS2,
        }));
    builder.AddHealthChecks().AddAzureCosmosDB();
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide `CosmosClient` instance.
- `optionsFactory`: A factory method to provide `AzureCosmosDbHealthCheckOptions` instance. It allows to specify the database id and/or container id(s).
- `healthCheckName`: The health check name. The default is `azure_cosmosdb`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new CosmosClient(
        "endpoint-from-portal",
        new DefaultAzureCredential(),
        new CosmosClientOptions()
        {
            ApplicationRegion = Regions.EastUS2,
        }));
    builder.AddHealthChecks().AddAzureCosmosDB(
        optionsFactory: sp => new CosmosDbHealthCheckOptions()
        {
            DatabaseId = "demo"
        });
}
```

### Breaking changes

In the prior releases, `CosmosDbHealthCheck` was a part of `HealthChecks.CosmosDb` package. It had a dependency on not just `Microsoft.Azure.Cosmos`, but also `Azure.Data.Tables`. The packages have been split to avoid bringing unnecessary dependencies. Moreover, `CosmosDbHealthCheck` was letting the users specify how `CosmosClient` should be created (from raw connection string or from endpoint and managed identity credentials), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `CosmosClient`** (please see [#2040](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2040) for more details). Since Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/> and client instances can be expensive to create, it's recommended to register a singleton factory method for Azure SDK clients. So the clients are created only when needed and once per whole application lifetime.


