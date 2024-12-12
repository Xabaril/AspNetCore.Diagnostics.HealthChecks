# RabbitMQ Health Check

This health check verifies the ability to communicate with a RabbitMQ server

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `rabbitmq`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Dependency Injected `IConnection`

As per [RabbitMQ docs](https://www.rabbitmq.com/connections.html) and its suggestions on
[high connectivity churn](https://www.rabbitmq.com/networking.html#dealing-with-high-connection-churn), connections are meant to be long lived.
Ideally, this should be configured as a singleton. The health check should use the same IConnection instance that is used in the application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://user:pass@host/vhost"),
            };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        })
        .AddHealthChecks()
        .AddRabbitMQ();
}
```

### Caching IConnection outside of Dependency Injection

Alternatively, you can create the connection outside of the dependency injection container and use it in the health check.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddRabbitMQ(sp => connectionTask.Value);
}

private static readonly Lazy<Task<IConnection>> connectionTask = new Lazy<Task<IConnection>>(CreateConnection);

private static async Task<IConnection> CreateConnection()
{
    var factory = new ConnectionFactory
    {
        Uri = new Uri("amqps://user:pass@host/vhost"),
    };
    return await factory.CreateConnectionAsync();
}
```

### Breaking changes

`RabbitMQHealthCheck` was letting the users specify how `IConnection` should be created (from raw connection string or from `Uri` or from `IConnectionFactory`), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `IConnection`** (please see [#2048](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2148) for more details). Since RabbitMQ [recommends](https://www.rabbitmq.com/client-libraries/dotnet-api-guide#connection-and-channel-lifespan) reusing client instances since they can be expensive to create, it's recommended to register a singleton factory method for `IConnection`. So the client is created only when needed and once per whole application lifetime.
