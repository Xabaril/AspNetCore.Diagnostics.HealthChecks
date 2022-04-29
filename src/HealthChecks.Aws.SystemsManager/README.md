# AWS Systems Manager Health Check

This health check verifies the ability to communicate with Amazon Systems Manager and the existence of some parameters on the Parameter Store. For more information about AWS Systems Mananger check the [AWS Systems Manager Site (Parameter Store)](https://aws.amazon.com/systems-manager/features/#Parameter_Store)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `aws systems manager`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

### Check existence of a parameter and load credentials from the application's default configuration

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSystemsManager(options =>
        {
            options.AddParameter("parameter-name");
        });
}
```

### Check existence of a parameter and directly pass credentials

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSystemsManager(options =>
        {
            options.AddParameter("parameter-name");
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
        });
}
```

### Check existence of a parameter and specify region endpoint

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSystemsManager(options =>
        {
            options.AddParameter("parameter-name");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

### Check existence of a parameter and specify credentials with region endpoint

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSystemsManager(options =>
        {
            options.AddParameter("parameter-name");
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```
