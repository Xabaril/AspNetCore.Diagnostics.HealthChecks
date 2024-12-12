## SurrealDB Health Check

This health check verifies the ability to communicate with [SurrealDb](https://surrealdb.com). It uses the provided [ISurrealDbClient](https://surrealdb.com/docs/sdk/dotnet).

### Defaults

By default, the `ISurrealDbClient` instance is resolved from service provider. 

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSurreal("Server=http://localhost:8000;Namespace=test;Database=test");
    builder.AddHealthChecks().AddSurreal();
}
```
