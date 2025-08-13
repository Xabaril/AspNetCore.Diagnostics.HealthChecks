# ActiveMq Health Check

This health check verifies the ability to communicate with a ActiveMQ server

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `activemq`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Dependency Injected `IConnection`

```csharp
 var connectAddress = new Amqp.Address("activemqhost",
     61616,
     "artemis",
     "artemis",
     "/",
     "amqp");

 var connectionFactory = new ConnectionFactory();
 var connection = await connectionFactory.CreateAsync(connectAddress);

 var webHostBuilder = new WebHostBuilder()
     .ConfigureServices(services =>
     {
         services
             .AddSingleton<IConnection>(connection)
             .AddHealthChecks()
             .AddActiveMQ(name: "activemq", tags: ["activemq"]);
     })
     .Configure(app =>
     {
         app.UseHealthChecks("/health", new HealthCheckOptions
         {
             Predicate = r => r.Tags.Contains("activemq")
         });
     });
```






