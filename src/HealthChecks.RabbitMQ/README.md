# RabbitMQ Health Check

This health check verifies the ability to communicate with a RabbitMQ server

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `rabbitmq`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

This will create a new `IConnection` and reuse on every request to get the health check result. Use
the extension method where you provide the `Uri` to connect with. You can optionally set the `SslOption` if needed.
IConnection created with this option use UseBackgroundThreadsForIO by default in order to gracefully shutdown on non reference IConnection by ServiceCollection.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddRabbitMQ(rabbitConnectionString:"amqps://user:pass@host1/vhost")
        .AddRabbitMQ(rabbitConnectionString:"amqps://user:pass@host2/vhost");
}
```

### Dependency Injected `IConnection`

As per [RabbitMQ docs](https://www.rabbitmq.com/connections.html) and its suggestions on
[high connectivity churn](https://www.rabbitmq.com/networking.html#dealing-with-high-connection-churn), connections are meant to be long lived.
Ideally, this should be configured as a singleton.

If you are sharing a single connection for every time a health check is requested,
you must ensure automatic recovery is enable so that the connection can be re-established if lost.

```cs
public void ConfigureServices(IServiceCollection services)
{
   

    services
        .AddSingleton<IConnection>(sp=>
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://user:pass@host/vhost"),
                AutomaticRecoveryEnabled = true
            };

            return  factory.CreateConnection();
        })
        .AddHealthChecks()
        .AddRabbitMQ();
}
```

Alternatively, you can specify the connection to use with a factory function given the `IServiceProvider`.

```cs
public void ConfigureServices(IServiceCollection services)
{
   

    services
        .AddHealthChecks()
        .AddRabbitMQ(sp =>
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://user:pass@host/vhost"),
                AutomaticRecoveryEnabled = true
            };

            return factory.CreateConnection();
        });
}
```