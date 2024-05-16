using HealthChecks.Azure.IoTHub;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="IoTHubRegistryManagerHealthCheck"/>.
/// </summary>
public static class IoTHubHealthChecksBuilderExtensions
{
    private const string NAME_REGISTRY_MANAGER = "iothub_registrymanager";
    private const string NAME_SERVICE_CLIENT = "iothub_serviceclient";

    /// <summary>
    /// Add a health check for Azure IoT Hub registry manager.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="registryManagerFactory">
    /// /// An optional factory to obtain <see cref="RegistryManager" /> instance.
    /// When not provided, <see cref="RegistryManager" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">A action to configure the Azure IoT Hub connection to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'iothub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureIoTHubRegistryManager(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, RegistryManager>? registryManagerFactory = default,
        Func<IServiceProvider, IotHubRegistryManagerOptions>? optionsFactory = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? NAME_REGISTRY_MANAGER,
           sp => new IoTHubRegistryManagerHealthCheck(
                registryManager: registryManagerFactory?.Invoke(sp) ?? sp.GetRequiredService<RegistryManager>(),
                options: optionsFactory?.Invoke(sp)),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure IoT Hub service client.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceClientFactory">
    /// /// An optional factory to obtain <see cref="ServiceClient" /> instance.
    /// When not provided, <see cref="ServiceClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">A action to configure the Azure IoT Hub connection to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'iothub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureIoTHubServiceClient(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, ServiceClient>? serviceClientFactory = default,
        Func<IServiceProvider, IotHubServiceClientOptions>? optionsFactory = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? NAME_SERVICE_CLIENT,
           sp => new IoTHubServiceClientHealthCheck(
                serviceClient: serviceClientFactory?.Invoke(sp) ?? sp.GetRequiredService<ServiceClient>(),
                options: optionsFactory?.Invoke(sp)),
           failureStatus,
           tags,
           timeout));
    }
}
