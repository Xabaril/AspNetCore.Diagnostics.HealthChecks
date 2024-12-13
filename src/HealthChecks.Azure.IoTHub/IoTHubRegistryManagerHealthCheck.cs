using Microsoft.Azure.Devices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.IoTHub;

public sealed class IoTHubRegistryManagerHealthCheck : IHealthCheck
{
    private readonly RegistryManager _registryManager;
    private readonly string? _readQuery;
    private readonly string? _writeDeviceId;

    public IoTHubRegistryManagerHealthCheck(RegistryManager registryManager, string? readQuery = default, string? writeDeviceId = default)
    {
        _registryManager = Guard.ThrowIfNull(registryManager);

        if (string.IsNullOrEmpty(readQuery) && string.IsNullOrEmpty(writeDeviceId))
        {
            throw new ArgumentException("Either readQuery or writeDeviceId has to be provided");
        }

        _readQuery = readQuery;
        _writeDeviceId = writeDeviceId;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(_writeDeviceId))
            {
                await ExecuteRegistryWriteCheckAsync(cancellationToken).ConfigureAwait(false);
            }
            else
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
        var query = _registryManager.CreateQuery(_readQuery!, 1);
        await query.GetNextAsJsonAsync().ConfigureAwait(false);
    }

    private async Task ExecuteRegistryWriteCheckAsync(CancellationToken cancellationToken)
    {
        string deviceId = _writeDeviceId!;
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
