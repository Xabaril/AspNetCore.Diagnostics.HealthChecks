## Azure Event Hubs Health Check

This health check verifies the ability to communicate with [Azure Event Hubs](https://azure.microsoft.com/services/event-hubs/). It uses the provided [EventHubProducerClient](https://learn.microsoft.com/dotnet/api/azure.messaging.eventhubs.producer.eventhubproducerclient) to get event hub properties.

### Defaults

By default, the `EventHubProducerClient` instance is resolved from service provider. 

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new EventHubProducerClient("fullyQualifiedNamespace", "eventHubName", new DefaultAzureCredential()));
    builder.AddHealthChecks().AddAzureEventHub();
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide `EventHubProducerClient` instance. This can be very useful when you need more than one `EventHubProducerClient` instance in your app (please see the example below that uses keyed DI introduced in .NET 8).
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddKeyedSingleton(serviceKey: "eventHubName1", (serviceProvider, serviceKey) => new EventHubProducerClient("fullyQualifiedNamespace", "eventHubName1", new DefaultAzureCredential()))
        .AddKeyedSingleton(serviceKey: "eventHubName2", (serviceProvider, serviceKey) => new EventHubProducerClient("fullyQualifiedNamespace", "eventHubName2", new DefaultAzureCredential()))
        .AddHealthChecks()
            .AddAzureKeyVaultSecrets(clientFactory: serviceProvider => serviceProvider.GetRequiredKeyedService<EventHubProducerClient>("eventHubName1"), healthCheckName: "event_hub_1")
            .AddAzureKeyVaultSecrets(clientFactory: serviceProvider => serviceProvider.GetRequiredKeyedService<EventHubProducerClient>("eventHubName2"), healthCheckName: "event_hub_2");

}
```


### Breaking changes

In the prior releases, `AzureEventHubHealthCheck` was a part of `HealthChecks.AzureServiceBus` package. It had a dependency on not just `Azure.Messaging.EventHubs`, but also `Azure.Messaging.ServiceBus`. The packages have been split to avoid bringing unnecessary dependencies. Moreover, `AzureEventHubHealthCheck` was letting the users specify how `EventHubProducerClient` should be created (from raw connection string or from fully qualified namespace and managed identity credentials), at a cost of maintaining an internal, static client instances cache. Now the type does not create client instances nor maintain an internal cache and **it's the caller responsibility to provide the instance of `EventHubProducerClient`** (please see [#2040](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2040) for more details). Since Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/> and client instances can be expensive to create, it's recommended to register a singleton factory method for Azure SDK clients. So the clients are created only when needed and once per whole application lifetime.
