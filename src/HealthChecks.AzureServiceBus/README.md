## Azure Service Bus Health Check

This health check verifies the ability to communicate with [Azure Service Bus](https://azure.microsoft.com/services/service-bus/). 

### Breaking changes

In the prior releases, `AzureEventHubHealthCheck` was a part of `AspNetCore.HealthChecks.AzureServiceBus` package. It had a dependency on not just `Azure.Messaging.ServiceBus`, but also `Azure.Messaging.EventHubs`. The packages have been split to avoid bringing unnecessary dependencies. The name of the new EveHub package is [AspNetCore.HealthChecks.Azure.Messaging.EventHubs](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Messaging.EventHubs).
