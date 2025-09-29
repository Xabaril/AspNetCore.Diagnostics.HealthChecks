# Azure Event Grid Health Check

This health check verifies the ability to communicate with [Azure Event Grid](https://azure.microsoft.com/services/event-grid/). It uses the provided [EventGridPublisherClient](https://learn.microsoft.com/dotnet/api/azure.messaging.eventgrid.eventgridpublisherclient) to send a ping event to verify connectivity.

## Implementation

The health check makes an actual call to Event Grid by sending a simple ping event. This verifies that:
1. The client is properly configured
2. The connection to Event Grid service is working
3. The topic exists and is accessible
4. The credentials are valid and not expired

## Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register the EventGridPublisherClient
    services.AddSingleton(sp =>
    {
        return new EventGridPublisherClient(
            new Uri("https://<your-topic-endpoint>"),
            new AzureKeyCredential("<your-access-key>"));
    });

    services
        .AddHealthChecks()
        .AddAzureEventGrid();
}
```

You can also register the health check by providing a factory method for the client:

```csharp
services
    .AddHealthChecks()
    .AddAzureEventGrid(sp =>
    {
        return new EventGridPublisherClient(
            new Uri("https://<your-topic-endpoint>"),
            new AzureKeyCredential("<your-access-key>"));
    });
```

## Parameters

- `clientFactory`: An optional factory to obtain the EventGridPublisherClient instance. When not provided, EventGridPublisherClient is resolved from IServiceProvider.
- `name`: The health check name. Optional. If null the name 'azure_event_grid' will be used.
- `failureStatus`: The HealthStatus that should be reported when the health check fails. Optional. If null then the default status of HealthStatus.Unhealthy will be reported.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: An optional TimeSpan representing the timeout of the check.