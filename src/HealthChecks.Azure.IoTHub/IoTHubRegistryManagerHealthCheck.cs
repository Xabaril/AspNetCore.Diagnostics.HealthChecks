using Microsoft.Azure.Devices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.IoTHub;

public sealed class IoTHubRegistryManagerHealthCheck : IHealthCheck
{
    private readonly IotHubRegistryManagerOptions _options;
    private readonly RegistryManager _registryManager;

    public IoTHubRegistryManagerHealthCheck(RegistryManager registryManager, IotHubRegistryManagerOptions? options = default)
    {
        _options = options ?? new IotHubRegistryManagerOptions();
        _registryManager = Guard.ThrowIfNull(registryManager);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.RegistryWriteCheck && !_options.RegistryReadCheck)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: $"No health check enabled, both {nameof(IotHubRegistryManagerOptions.RegistryReadCheck)} and {nameof(IotHubRegistryManagerOptions.RegistryWriteCheck)} are false");
        }

        try
        {
            if (_options.RegistryWriteCheck)
            {
                await ExecuteRegistryWriteCheckAsync(cancellationToken).ConfigureAwait(false);
            }
            else if (_options.RegistryReadCheck)
            {
                await ExecuteRegistryReadCheckAsync().ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }


    private async Task ExecuteRegistryReadCheckAsync()
    {
        var query = _registryManager.CreateQuery(_options.RegistryReadQuery, 1);
        await query.GetNextAsJsonAsync().ConfigureAwait(false);
    }

    private async Task ExecuteRegistryWriteCheckAsync(CancellationToken cancellationToken)
    {
        var deviceId = _options.RegistryWriteDeviceIdFactory();
        var device = await _registryManager.GetDeviceAsync(deviceId, cancellationToken).ConfigureAwait(false);

        // in default implementation of configuration deviceId equals "health-check-registry-write-device-id"
        // if in previous health check device were not removed -- try remove it
        // if in previous health check device were added and removed -- try create and remove it
        if (device != null)
        {
            await _registryManager.RemoveDeviceAsync(deviceId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _registryManager.AddDeviceAsync(new Device(deviceId), cancellationToken).ConfigureAwait(false);
            await _registryManager.RemoveDeviceAsync(deviceId, cancellationToken).ConfigureAwait(false);
        }
    }
}
