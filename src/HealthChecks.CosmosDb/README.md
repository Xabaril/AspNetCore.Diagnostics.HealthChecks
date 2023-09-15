## Azure Tables Health Check

This health check verifies the ability to communicate with [Azure Cosmos DB](https://azure.microsoft.com/en-us/products/cosmos-db/). It uses the provided [CosmosClient](https://learn.microsoft.com/dotnet/api/microsoft.azure.cosmos.cosmosclient).

### Defaults

By default, the `CosmosClient` instance is resolved from service provider. `AzureCosmosDbHealthCheckOptions` does not provide any specific containers or database ids, the health check just calls [CosmosClient.ReadAccountAsync](https://learn.microsoft.com/dotnet/api/microsoft.azure.cosmos.cosmosclient.readaccountasync).

```csharp
public void Configure(IHealthChecksBuilder builder)
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
public void Configure(IHealthChecksBuilder builder)
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
