# InfluxDB Health Check

This health check verifies the ability to communicate with a InfluxDB server.

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `InfluxDB`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

This will create a new `InfluxDBClient` and reuse it on every request to get the health check result. Use
the extension method where you provide the `Uri` to connect with. 

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
            .AddInfluxDB("http://localhost:8086/?org=iotsharp&bucket=iotsharp-bucket&token=iotsharp-token");
}
```

If you are sharing a single InfluxDBClient for every time a health check is requested,
you must ensure automatic recovery is enable so that the InfluxDBClient can be re-established if lost.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<InfluxDBClient>(sp=>
              {
                 return  InfluxDBClientFactory.Create("http://localhost:8086/?org=iotsharp&bucket=iotsharp-bucket&token=iotsharp-token");
              })
            .AddHealthChecks()
            .AddInfluxDB();
}
```
