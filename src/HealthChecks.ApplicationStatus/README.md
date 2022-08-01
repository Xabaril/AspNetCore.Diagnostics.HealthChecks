# ArangoDb Health Check

This health check verifies that application is up and runnning based on IHostApplicationLifetime.
If application recieved stop signal, eg: SIGTERM in docker container - then health status will be unhealthy and
application won't be able to recieve new requests.

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `applicationstatus`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

```csharp
public void ConfigureServices(IServiceCollection services, IHost)
{
    services
        .AddHealthChecks()
        .AddCheck<ApplicationStatusHealthCheck>();
}
```
