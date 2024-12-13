using HealthChecks.Azure.IoTHub;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="IoTHubRegistryManagerHealthCheck"/>.
/// </summary>
public static class IoTHubHealthChecksBuilderExtensions
{
    private const string NAME_REGISTRY_MANAGER_READ = "iothub_registrymanager_read";
    private const string NAME_REGISTRY_MANAGER_WRITE = "iothub_registrymanager_write";
    private const string NAME_SERVICE_CLIENT = "iothub_serviceclient";

    /// <summary>
    /// Adds a read health check for Azure IoT Hub registry manager.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="registryManagerFactory">
    /// An optional factory to obtain <see cref="RegistryManager" /> instance.
    /// When not provided, <see cref="RegistryManager" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="query">The query to perform.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'iothub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureIoTHubRegistryReadCheck(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, RegistryManager>? registryManagerFactory = default,
        string query = "SELECT deviceId FROM devices",
        string? name = NAME_REGISTRY_MANAGER_READ,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(query);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME_REGISTRY_MANAGER_READ,
           sp => new IoTHubRegistryManagerHealthCheck(
                registryManager: registryManagerFactory?.Invoke(sp) ?? sp.GetRequiredService<RegistryManager>(),
                readQuery: query),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a write health check for Azure IoT Hub registry manager.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="registryManagerFactory">
    /// An optional factory to obtain <see cref="RegistryManager" /> instance.
    /// When not provided, <see cref="RegistryManager" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="deviceId">The id of the device to add and remove.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'iothub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureIoTHubRegistryWriteCheck(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, RegistryManager>? registryManagerFactory = default,
        string deviceId = "health-check-registry-write-device-id",
        string? name = NAME_REGISTRY_MANAGER_WRITE,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(deviceId);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME_REGISTRY_MANAGER_WRITE,
           sp => new IoTHubRegistryManagerHealthCheck(
                registryManager: registryManagerFactory?.Invoke(sp) ?? sp.GetRequiredService<RegistryManager>(),
                writeDeviceId: deviceId),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure IoT Hub service client.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceClientFactory">
    /// An optional factory to obtain <see cref="ServiceClient" /> instance.
    /// When not provided, <see cref="ServiceClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
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
        string? name = NAME_SERVICE_CLIENT,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? NAME_SERVICE_CLIENT,
           sp => new IoTHubServiceClientHealthCheck(serviceClient: serviceClientFactory?.Invoke(sp) ?? sp.GetRequiredService<ServiceClient>()),
           failureStatus,
           tags,
           timeout));
    }
}
