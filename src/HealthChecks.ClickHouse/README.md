## PostgreSQL Health Check

This health check verifies the ability to communicate with [PostgreSQL](https://www.postgresql.org/). It uses the [Npgsql](https://www.npgsql.org/) library.

## NpgsqlDataSource

Starting with Npgsql 7.0 (and .NET 7), the starting point for any database operation is [NpgsqlDataSource](https://www.npgsql.org/doc/api/Npgsql.NpgsqlDataSource.html). The data source represents your PostgreSQL database, and can hand out connections to it, or support direct execution of SQL against it. The data source encapsulates the various Npgsql configuration needed to connect to PostgreSQL, as well as the **connection pooling which makes Npgsql efficient**.

Npgsql's **data source supports additional configuration beyond the connection string**, such as logging, advanced authentication options, type mapping management, and more.

## Recommended approach

To take advantage of the performance `NpgsqlDataSource` has to offer, it should be used as a **singleton**. Otherwise, the app might end up with having multiple data source instances, all of which would have their own connection pools. This can lead to resources exhaustion and major performance issues (Example: [#1993](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/1993)).

We encourage you to use [Npgsql.DependencyInjection](https://www.nuget.org/packages/Npgsql.DependencyInjection) package for registering a singleton factory for `NpgsqlDataSource`.  It allows easy configuration of your Npgsql connections and registers the appropriate services in your DI container.

To make the shift to `NpgsqlDataSource` as easy as possible,  the `Npgsql.DependencyInjection` package registers not just a factory for the data source, but also factory for `NpgsqlConnection`  (and even `DbConnection`). So, your app does not need to suddenly start using `NpgsqlDataSource` everywhere.

```csharp
void Configure(IServiceCollection services)
{
    services.AddNpgsqlDataSource("Host=pg_server;Username=test;Password=test;Database=test");
    services.AddHealthChecks().AddNpgSql();
}
```

By default, the `NpgsqlDataSource` instance is resolved from service provider. If you need to access more than one PostgreSQL database, you can use keyed DI services to achieve that:

```csharp
void Configure(IServiceCollection services)
{
    services.AddNpgsqlDataSource("Host=pg_server;Username=test;Password=test;Database=first", serviceKey: "first");
    services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredKeyedService<NpgsqlDataSource>("first"));

    services.AddNpgsqlDataSource("Host=pg_server;Username=test;Password=test;Database=second", serviceKey: "second");
    services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredKeyedService<NpgsqlDataSource>("second"));
}
```


## Connection String

Raw connection string is of course still supported:

```csharp
services.AddHealthChecks().AddNpgSql("Host=pg_server;Username=test;Password=test;Database=test");
```
