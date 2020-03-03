# IbmMQ Health Check

This health check verifies the ability to communicate with a IbmMQ 9.0.+ server

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `ibmmq`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

Use the extension method where you provide the queue manager name and the connection properties.

```cs
public void ConfigureServices(IServiceCollection services)
{
    Hashtable connectionProperties = new Hashtable {
        {MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_BINDINGS},
    };
    services
        .AddHealthChecks()
        .AddIbmMQ("QM.TEST.01", connectionProperties);
}
```

### Using Managed Client Connection

For `MQC.TRANSPORT_MQSERIES_MANAGED` connection you can use the following conveniece extension method where you need to specify the channel and the host(port) information. User and password are optional parameters.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddIbmMQManagedConnection("QM.TEST.01", "DEV.APP.SVRCONN", "localhost(1417)", userName: "app", password: "xxx");
}
```
