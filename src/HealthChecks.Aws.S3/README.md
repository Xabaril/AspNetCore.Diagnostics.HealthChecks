# Amazon S3 Health Check

This health check verifies the ability to communicate with [Amazon S3](https://aws.amazon.com/s3/).

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `aws s3`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Load credentials from the application's default configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddS3(options =>
        {
            options.BucketName = "bucket-name";
        });
}
```

### Directly pass credentials

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddS3(options =>
        {
            options.BucketName = "bucket-name";
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
        });
}
```

### Specify region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddS3(options =>
        {
            options.BucketName = "bucket-name";
            options.S3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.EUCentral1
            };
        });
}
```
