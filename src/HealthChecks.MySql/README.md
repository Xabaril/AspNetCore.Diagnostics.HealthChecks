## MySQL Health Check

This health check verifies the ability to communicate with a MySQL Server.
It uses the provided [MySqlDataSource](https://mysqlconnector.net/api/mysqlconnector/mysqldatasourcetype/) or a connection string to connect to the server.

### Defaults

By default, the `MySqlDataSource` instance is resolved from service provider.
(This should be the same as the instance being used by the application; do not create a new `MySqlDataSource` just for the health check.)
The health check will send a MySQL "ping" packet to the server to verify connectivity.

```csharp
builder.Services
    .AddMySqlDataSource(builder.Configuration.GetConnectionString("mysql")) // using the MySqlConnector.DependencyInjection package
    .AddHealthChecks().AddMySql();
```

### Connection String

You can also specify a connection string directly:

```csharp
builder.Services.AddHealthChecks().AddMySql(connectionString: "Server=...;User Id=...;Password=...");
```

This can be useful if you're not using `MySqlDataSource` in your application.

### Customization

You can additionally add the following parameters:

- `healthQuery`: A query to run against the server. If `null` (the default), the health check will send a MySQL "ping" packet to the server.
- `configure`: An action to configure the `MySqlConnection` object. This is called after the `MySqlConnection` is created but before the connection is opened.
- `name`: The health check name. The default is `mysql`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
builder.Services
    .AddMySqlDataSource(builder.Configuration.GetConnectionString("mysql"))
    .AddHealthChecks().AddMySql(
        healthQuery: "SELECT 1;",
        configure: conn => conn.ConnectTimeout = 3,
        name: "MySQL"
    );
```

### Breaking changes

In previous versions, `MySqlHealthCheck` defaulted to testing connectivity by sending a `SELECT 1;` query to the server.
It has been changed to send a more efficient "ping" packet instead.
To restore the previous behavior, specify `healthQuery: "SELECT 1;"` when registering the health check.

While not a breaking change, it's now preferred to use `MySqlDataSource` instead of a connection string.
This allows the health check to use the same connection pool as the rest of the application.
This can be achieved by calling the `.AddMySql()` overload that has no required parameters.
The health check assumes that a `MySqlDataSource` instance has been registered with the service provider and will retrieve it automatically.

