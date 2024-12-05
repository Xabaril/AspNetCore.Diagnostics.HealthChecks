## MongoDB Health Check

This health check verifies the ability to communicate with [MongoDB](https://www.mongodb.com/). It uses the provided [MongoClient](https://www.mongodb.com/docs/drivers/csharp/current/) to list database names or ping configured database.

### Defaults

By default, the `MongoClient` instance is resolved from service provider. 

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddSingleton(sp => new MongoClient("mongodb://localhost:27017"))
        .AddHealthChecks()
        .AddMongoDb();
}
```

### Customization

You can additionally add the following parameters:

- `mongoClientFactory`: A factory method to provide `MongoClient` instance.
- `mongoDatabaseNameFactory`: A factory method to provide database name.
- `name`: The health check name. The default is `mongodb`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddSingleton(sp => new MongoClient("mongodb://localhost:27017"))
        .AddHealthChecks()
        .AddMongoDb(mongoDatabaseNameFactory: sp => "theName");
}
```

### Breaking changes

`MongoDbHealthCheck` was letting the users specify how `MongoClient` should be created (from raw connection string or from `MongoUrl` or from `MongoClientSettings`), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `MongoClient`** (please see [#2048](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2148) for more details). Since MongoDB [recommends](https://mongodb.github.io/mongo-csharp-driver/2.17/reference/driver/connecting#re-use) treating clients as singletons and client instances can be expensive to create, it's recommended to register a singleton factory method for `MongoClient`. So the client is created only when needed and once per whole application lifetime.

