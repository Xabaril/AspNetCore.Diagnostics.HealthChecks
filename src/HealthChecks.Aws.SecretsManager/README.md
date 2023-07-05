# AWS Secrets Manager Health Check

This health check verifies the ability to communicate with Amazon Secrets Manager and the existence of some secrets. For more information about AWS Secrets Manager check the [AWS Secrets Manager Site](https://aws.amazon.com/secrets-manager/)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `aws secrets manager`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

### Check existence of a secret and load credentials from the application's default configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSecretsManager(options =>
        {
            options.AddSecret("secretname");
        });
}
```

### Check existence of a secret and directly pass credentials

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSecretsManager(options =>
        {
            options.AddSecret("secretname");
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
        });
}
```

### Check existence of a secret and specify region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSecretsManager(options =>
        {
            options.AddSecret("secretname");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

### Check existence of a secret and specify credentials with region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSecretsManager(options =>
        {
            options.AddSecret("secretname");
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```
