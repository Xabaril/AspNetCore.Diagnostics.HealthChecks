using Microsoft.Azure.Devices;
using System;

namespace HealthChecks.Azure.IoTHub
{
    public class IoTHubOptions
    {
        internal string ConnectionString { get; private set; }
        internal bool RegistryReadCheck { get; private set; }
        internal bool RegistryWriteCheck { get; private set; }
        internal bool ServiceConnectionCheck { get; private set; }
        internal string RegistryReadQuery { get; private set; }
        internal Func<string> RegistryWriteDeviceIdFactory { get; private set; }
        internal TransportType ServiceConnectionTransport { get; private set; }

        public IoTHubOptions AddConnectionString(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }
        public IoTHubOptions AddRegistryReadCheck(string query = "SELECT deviceId FROM devices")
        {
            RegistryReadCheck = true;
            RegistryReadQuery = query;
            return this;
        }
        public IoTHubOptions AddRegistryWriteCheck(Func<string> deviceIdFactory = null)
        {
            RegistryWriteCheck = true;
            RegistryWriteDeviceIdFactory = deviceIdFactory ?? (() => "health-check-registry-write-device-id");
            return this;
        }
        public IoTHubOptions AddServiceConnectionCheck(TransportType transport = TransportType.Amqp)
        {
            ServiceConnectionCheck = true;
            ServiceConnectionTransport = transport;
            return this;
        }
    }
}