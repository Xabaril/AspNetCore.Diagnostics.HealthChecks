using Azure.Identity;
using Microsoft.Azure.Devices;

namespace HealthChecks.Azure.IoTHub.Tests;

public class IoTHubRegistryManagerConformanceTests : ConformanceTests<RegistryManager, IoTHubRegistryManagerHealthCheck, IotHubRegistryManagerOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, RegistryManager>? clientFactory = null, Func<IServiceProvider, IotHubRegistryManagerOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureIoTHubRegistryManager(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override RegistryManager CreateClientForNonExistingEndpoint()
    {
        HttpTransportSettings settings = new();

        return RegistryManager.Create("thisisnotarealurl", new DefaultAzureCredential(), settings);
    }

    protected override IoTHubRegistryManagerHealthCheck CreateHealthCheck(RegistryManager client, IotHubRegistryManagerOptions? options)
        => new(client, options);

    protected override IotHubRegistryManagerOptions CreateHealthCheckOptions()
        => new IotHubRegistryManagerOptions().AddRegistryReadCheck().AddRegistryWriteCheck();
}
