# AWS S3 Health Check

This health check verifies the ability to communicate with Amazon AWS S3. For more information about Amazon AWS S3 please check and .NET please check the [Github Project](https://github.com/aws/aws-sdk-net/)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `aws s3`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddS3(options=>
        {
            BucketName = "bucket-name",
            AccessKey = "access-key"
        });
}
```