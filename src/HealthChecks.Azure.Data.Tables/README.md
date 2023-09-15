## Azure Tables Health Check

This health check verifies the ability to communicate with [Azure Tables](https://azure.microsoft.com/en-us/products/storage/tables/). It uses the provided [TableServiceClient](https://learn.microsoft.com/dotnet/api/azure.data.tables.tableserviceclient).

### Defaults

By default, the `TableServiceClient` instance is resolved from service provider. `AzureTableServiceHealthCheckOptions` does not provide any specific container name, so the health check fetches just first container.

```csharp
public void Configure(IHealthChecksBuilder builder)
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
public void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new TableServiceClient(new Uri("azure-table-storage-uri"), new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureTable(
        optionsFactory: sp => new AzureTableServiceHealthCheckOptions()
        {
            TableName = "demo"
        });
}
```

For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme)
