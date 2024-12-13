using Azure.Identity;
using Microsoft.Azure.Devices;

namespace HealthChecks.Azure.IoTHub.Tests;

public class IoTHubRegistryManagerConformanceTests_Read : ConformanceTests<RegistryManager, IoTHubRegistryManagerHealthCheck, IotHubRegistryManagerOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, RegistryManager>? clientFactory = null, Func<IServiceProvider, IotHubRegistryManagerOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureIoTHubRegistryReadCheck(clientFactory, name: healthCheckName, failureStatus: failureStatus, tags: tags, timeout: timeout);

    protected override RegistryManager CreateClientForNonExistingEndpoint()
    {
        HttpTransportSettings settings = new();

        return RegistryManager.Create("thisisnotarealurl", new DefaultAzureCredential(), settings);
    }

    protected override IoTHubRegistryManagerHealthCheck CreateHealthCheck(RegistryManager client, IotHubRegistryManagerOptions? options)
        => new(client, readQuery: "SELECT deviceId FROM devices");

    protected override IotHubRegistryManagerOptions CreateHealthCheckOptions()
        => new();
}

public class IoTHubRegistryManagerConformanceTests_Write : ConformanceTests<RegistryManager, IoTHubRegistryManagerHealthCheck, IotHubRegistryManagerOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, RegistryManager>? clientFactory = null, Func<IServiceProvider, IotHubRegistryManagerOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureIoTHubRegistryWriteCheck(clientFactory, name: healthCheckName, failureStatus: failureStatus, tags: tags, timeout: timeout);

    protected override RegistryManager CreateClientForNonExistingEndpoint()
    {
        HttpTransportSettings settings = new();

        return RegistryManager.Create("thisisnotarealurl", new DefaultAzureCredential(), settings);
    }

    protected override IoTHubRegistryManagerHealthCheck CreateHealthCheck(RegistryManager client, IotHubRegistryManagerOptions? options)
        => new(client, writeDeviceId: "some-id");

    protected override IotHubRegistryManagerOptions CreateHealthCheckOptions()
        => new();
}

public sealed class IotHubRegistryManagerOptions;
