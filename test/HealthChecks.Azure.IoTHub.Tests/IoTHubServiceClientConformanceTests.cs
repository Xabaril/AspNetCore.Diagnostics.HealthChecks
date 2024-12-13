using Azure.Identity;
using Microsoft.Azure.Devices;

namespace HealthChecks.Azure.IoTHub.Tests;

public class IoTHubServiceClientConformanceTests : ConformanceTests<ServiceClient, IoTHubServiceClientHealthCheck, IotHubServiceClientOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, ServiceClient>? clientFactory = null, Func<IServiceProvider, IotHubServiceClientOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureIoTHubServiceClient(clientFactory, healthCheckName, failureStatus, tags, timeout);

    protected override ServiceClient CreateClientForNonExistingEndpoint()
    {
        return ServiceClient.Create("thisisnotarealurl", new DefaultAzureCredential());
    }

    protected override IoTHubServiceClientHealthCheck CreateHealthCheck(ServiceClient client, IotHubServiceClientOptions? options)
        => new(client);

    protected override IotHubServiceClientOptions CreateHealthCheckOptions()
        => new();
}

public sealed class IotHubServiceClientOptions;
