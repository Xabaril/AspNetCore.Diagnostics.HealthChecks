## OpenTelemetry HealthCheck Publisher

This publisher sends health reports as OpenTelemetry metrics.

Every time the OpenTelemetry exporter sends a batch, it reads the latest available health report and emits each health check as two gauge instruments, `health_check.status` and `health_check.duration`.

- `health_check.status` – Represents the health status. It is mapped in the following way:

    | HealthStatus | Value |
    |--------------|-------|
    | Healthy      | 1     |
    | Degraded     | 0.5   |
    | Unhealthy    | 0     |


- `health_check.duration` - The time (in seconds) it took to run the health check.

Each metric includes the following tag:
- `health_check.name` - The distinct name of the health check.

### Note
The batching interval is controlled by the `OTEL_METRIC_EXPORT_INTERVAL` environment variable. It defaults to 60000 (60 seconds).

OpenTelemetry exporters must be configured separately.

## Usage

To enable OpenTelemetry publishing for health checks, call the provided extension method:
```
IHealthChecksBuilder AddOpenTelemetryPublisher(this IHealthChecksBuilder builder)
```

This method also configures instrumentation for the `Microsoft.AspNetCore.Diagnostics.HealthChecks` Meter.

### Example

```csharp
public void ConfigureHealthChecks(IServiceCollection services) 
{
    services.Configure<HealthCheckPublisherOptions>(options =>
    {
        options.Delay = TimeSpan.FromSeconds(5);
        options.Period = TimeSpan.FromSeconds(30);
    });

    services
        .AddHealthChecks()
        .AddOpenTelemetryPublisher();
}
```

