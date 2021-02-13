# ArangoDb Health Check

This health check verifies the ability to communicate with a ArangoDb server. ArangoDb is a Hight Available and Multi-Model database.
For more information about ArangoDb please check [ArangoDb Home](https://www.arangodb.com/)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `arangodb`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddArangoDb(_ => new ArangoDbOptions
                      {
                          HostUri = "http://localhost:8529/",
                          Database = "_system",
                          UserName = "root",
                          Password = "strongArangoDbPassword"
                      });
}
```