## ClickHouse Health Check

This health check verifies the ability to communicate with [ClickHouse](https://www.clickhouse.com/). It uses the [ClickHouse.Client](https://www.nuget.org/packages/ClickHouse.Client) library.

## Recommended approach

When registering the ClickHouse health check, it is recommended to use a shared `HttpClient` instance so that you can take advantage of the built-in [pooling](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines#pooled-connections).

We encourage you to use [ClickHouse.Extensions.DependencyInjection](https://www.nuget.org/packages/ClickHouse.Extensions.DependencyInjection) package. It allows easy configuration of your ClickHouse connections and registers the appropriate services in your DI container.

```csharp
void Configure(IServiceCollection services)
{
    services.AddClickHouseDataSource("Host=ch;Username=default;Password=test;Database=default");
    services.AddHealthChecks().AddClickHouse(static sp => sp.GetRequiredService<ClickHouseDataSource>().CreateConnection());
}
```

If you need to access more than one ClickHouse databases, you can use keyed DI services to achieve that:

```csharp
void Configure(IServiceCollection services)
{
    services.AddClickHouseDataSource("Host=ch;Username=default;Password=test;Database=first", serviceKey: "first");
    services.AddHealthChecks().AddClickHouse(static sp => sp.GetRequiredKeyedService<ClickHouseDataSource>("first").CreateConnection());

    services.AddClickHouseDataSource("Host=ch;Username=default;Password=test;Database=second", serviceKey: "second");
    services.AddHealthChecks().AddClickHouse(static sp => sp.GetRequiredKeyedService<ClickHouseDataSource>("second").CreateConnection());
}
```


## Connection String

A raw connection string can also be used. This approach is **not** recommended given that it will not take advantage of any pooling within the underlying `HttpClient`.

```csharp
services.AddHealthChecks().AddClickHouse("Host=ch;Username=default;Password=test;Database=default");
```
