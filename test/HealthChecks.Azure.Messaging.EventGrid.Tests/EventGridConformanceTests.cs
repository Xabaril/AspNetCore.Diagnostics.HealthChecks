using Azure.Identity;
using Azure.Messaging.EventGrid;

namespace HealthChecks.Azure.Messaging.EventGrid.Tests;

public class EventGridConformanceTests : ConformanceTests<EventGridPublisherClient, AzureEventGridHealthCheck, UnusedOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, EventGridPublisherClient>? clientFactory = null, Func<IServiceProvider, UnusedOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureEventGrid(clientFactory, healthCheckName, failureStatus, tags, timeout);

    protected override EventGridPublisherClient CreateClientForNonExistingEndpoint()
        => new(new Uri("https://non-existing-topic.region.eventgrid.azure.net"), new AzureCliCredential());

    protected override AzureEventGridHealthCheck CreateHealthCheck(EventGridPublisherClient client, UnusedOptions? options)
        => new(client);

    protected override UnusedOptions CreateHealthCheckOptions()
        => new();
}

// AzureEventGridHealthCheck does not use any options, the type exists only to meet ConformanceTests<,,> criteria
public sealed class UnusedOptions
{
}