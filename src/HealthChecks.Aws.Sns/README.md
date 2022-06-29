# AWS SNS Health Check

This health check verifies the ability to communicate with Amazon SNS and the existence of some topics and its subscriptions. For more information about AWS SNS check the [AWS SNS Site](https://aws.amazon.com/sns/)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `aws sns`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Check existence of a topic and loads credentials from the application's default configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddTopicAndSubscriptions(options =>
        {
            options.AddTopic("topicName");
        });
}
```

### Check existence of a topic and directly pass credentials

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddTopicAndSubscriptions(options =>
        {
            options.AddTopic("topicName");
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
        });
}
```

### Check existence of a topic and specify region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddTopicAndSubscriptions(options =>
        {
            options.AddTopic("topicName");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

### Check existence of a topic and specify credentials with region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddTopicAndSubscriptions(options =>
        {
            options.AddTopic("topicName");
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

### Check existence of a topic, its subscriptions and loads credentials from the application's default configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSnsSubscriptions(options =>
        {
             options.AddTopicAndSubscriptions("topicName", new string[] { "subscription1-arn", "subscription2-arn" });
        });
}
```

### Check existence of a topic, its subscriptions and directly pass credentials

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSnsSubscriptions(options =>
        {
            options.AddTopicAndSubscriptions("topicName", new string[] { "subscription1-arn", "subscription2-arn" });
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
        });
}
```

### Check existence of a topic, its subscriptions and specify region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSnsSubscriptions(options =>
        {
            options.AddTopicAndSubscriptions("topicName", new string[] { "subscription1-arn", "subscription2-arn" });
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

### Check existence of a topic, its subscriptions and specify credentials with region endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSnsSubscriptions(options =>
        {
            options.AddTopicAndSubscriptions("topicName", new string[] { "subscription1-arn", "subscription2-arn" });
            options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
            options.RegionEndpoint = RegionEndpoint.EUCentral1;
        });
}
```

