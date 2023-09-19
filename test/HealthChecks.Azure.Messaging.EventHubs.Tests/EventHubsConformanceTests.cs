using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;

namespace HealthChecks.Azure.Messaging.EventHubs.Tests;

public class EventHubsConformanceTests : ConformanceTests<EventHubProducerClient, AzureEventHubHealthCheck, UnusedOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, EventHubProducerClient>? clientFactory = null, Func<IServiceProvider, UnusedOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureEventHub(clientFactory, healthCheckName, failureStatus, tags, timeout);

    protected override EventHubProducerClient CreateClientForNonExistingEndpoint()
        => new("fullyQualifiedNamespace", "eventHubName", new AzureCliCredential());

    protected override AzureEventHubHealthCheck CreateHealthCheck(EventHubProducerClient client, UnusedOptions? options)
        => new(client);

    protected override UnusedOptions CreateHealthCheckOptions()
        => new();
}

// AzureEventHubHealthCheck does not use any options, the type exists only to meet ConformanceTests<,,> criteria
public sealed class UnusedOptions
{
}
