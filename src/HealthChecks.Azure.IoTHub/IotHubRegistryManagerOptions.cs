namespace HealthChecks.Azure.IoTHub;

public sealed class IotHubRegistryManagerOptions
{
    internal bool RegistryReadCheck { get; private set; }
    internal bool RegistryWriteCheck { get; private set; }
    internal string RegistryReadQuery { get; private set; } = null!;
    internal Func<string> RegistryWriteDeviceIdFactory { get; private set; } = null!;

    public IotHubRegistryManagerOptions AddRegistryReadCheck(string query = "SELECT deviceId FROM devices")
    {
        RegistryReadCheck = true;
        RegistryReadQuery = query;
        return this;
    }

    public IotHubRegistryManagerOptions AddRegistryWriteCheck(Func<string>? deviceIdFactory = null)
    {
        RegistryWriteCheck = true;
        RegistryWriteDeviceIdFactory = deviceIdFactory ?? (() => "health-check-registry-write-device-id");
        return this;
    }
}
