using System;

namespace HealthChecks.Azure.IoTHub
{
    public class IoTHubOptions
    {
        internal string ConnectionString { get; }
        internal bool RegistryReadCheck { get; private set; }
        internal bool RegistryWriteCheck { get; private set; }
        internal bool ServiceConnectionCheck { get; private set; }
        internal string RegistryReadQuery { get; private set; }
        internal Func<string> RegistryWriteDeviceIdFactory { get; private set; }
        internal ServiceConnectionTransport ServiceConnectionTransport { get; set; }

        public IoTHubOptions(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            ConnectionString = connectionString;
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
        public IoTHubOptions AddServiceConnectionCheck(ServiceConnectionTransport transport = ServiceConnectionTransport.Amqp)
        {
            ServiceConnectionCheck = true;
            ServiceConnectionTransport = transport;
            return this;
        }
    }
}