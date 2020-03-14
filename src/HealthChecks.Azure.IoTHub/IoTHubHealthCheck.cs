using Microsoft.Azure.Devices;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Azure.IoTHub
{
    public class IoTHubHealthCheck : IHealthCheck
    {
        private readonly IoTHubOptions _options;

        public IoTHubHealthCheck(IoTHubOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_options.RegistryWriteCheck)
                {
                    await ExecuteRegistryWriteCheckAsync(cancellationToken);
                }
                else if (_options.RegistryReadCheck)
                {
                    await ExecuteRegistryReadCheckAsync();
                }
                if (_options.ServiceConnectionCheck)
                {
                    await ExecuteServiceConnectionCheckAsync(cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private async Task ExecuteServiceConnectionCheckAsync(CancellationToken cancellationToken)
        {
            using (var client = ServiceClient.CreateFromConnectionString(_options.ConnectionString, _options.ServiceConnectionTransport))
            {
                await client.GetServiceStatisticsAsync(cancellationToken);
            }
        }

        private async Task ExecuteRegistryReadCheckAsync()
        {
            using (var client = RegistryManager.CreateFromConnectionString(_options.ConnectionString))
            {
                var query = client.CreateQuery(_options.RegistryReadQuery, 1);
                await query.GetNextAsJsonAsync();
            }
        }

        private async Task ExecuteRegistryWriteCheckAsync(CancellationToken cancellationToken)
        {
            using (var client = RegistryManager.CreateFromConnectionString(_options.ConnectionString))
            {
                var deviceId = _options.RegistryWriteDeviceIdFactory();
                var device = await client.GetDeviceAsync(deviceId, cancellationToken);

                // in default implementation of configuration deviceId equals "health-check-registry-write-device-id"
                // if in previous health check device were not removed -- try remove it
                // if in previous health check device were added and removed -- try create and remove it
                if (device != null)
                {
                    await client.RemoveDeviceAsync(deviceId, cancellationToken);
                }
                else
                {
                    await client.AddDeviceAsync(new Device(deviceId), cancellationToken);
                    await client.RemoveDeviceAsync(deviceId, cancellationToken);
                }
            }
        }
    }
}