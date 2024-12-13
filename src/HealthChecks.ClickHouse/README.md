## ClickHouse Health Check

This health check verifies the ability to communicate with [ClickHouse](https://www.clickhouse.com/). It uses the [ClickHouse.Client](https://www.nuget.org/packages/ClickHouse.Client) library.

## Recommended approach

When registering the ClickHouse health check, it is [recommended](https://github.com/DarkWanderer/ClickHouse.Client/wiki/Connection-lifetime-&-pooling#recommendations) to use `IHttpClientFactory` or a static instance of `HttpClient` to manage connections.

```csharp
void Configure(IServiceCollection services)
{
    services.AddHttpClient("ClickHouseClient");
    services.AddHealthChecks().AddClickHouse(static sp => {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        return new ClickHouseConnection("Host=ch;Username=default;Password=test;Database=default", httpClientFactory, "ClickHouseClient");
    });
}
```
