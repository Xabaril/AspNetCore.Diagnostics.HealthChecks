# Amazon DynamoDB Health Check

This health check verifies the ability to communicate with [Amazon DynamoDB](https://aws.amazon.com/dynamodb/).

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `dynamodb`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Load credentials from the application's default configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddDynamoDb(options =>
        {
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

### Directly pass credentials

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddDynamoDb(options =>
        {
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```
