using System.Runtime.CompilerServices;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;

[assembly: TypeForwardedTo(typeof(AzureServiceBusHealthCheck<>))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusQueueHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusQueueHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusQueueMessageCountThresholdHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusSubscriptionHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusSubscriptionHealthCheckHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusTopicHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureServiceBusTopicHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(ServiceBusClientProvider))]
[assembly: TypeForwardedTo(typeof(AzureEventHubHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureEventHubHealthCheckOptions))]
