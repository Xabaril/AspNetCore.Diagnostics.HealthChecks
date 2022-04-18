# Azure Application Insights Health Check

This health check verifies the existence of a Azure Application Insights resource. For more information about Azure Application Insights check the [Azure AppInsights Microsoft Site](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `azureappinsights`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureApplicationInsights("instrumentationKey");
}
```
