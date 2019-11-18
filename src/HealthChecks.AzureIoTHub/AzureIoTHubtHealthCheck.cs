using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace HealthChecks.AzureIoTHub
{
    public class AzureIoTHubHealthCheck : IHealthCheck
    {
        private readonly AzureIoTHubOptions _options;
        public AzureIoTHubHealthCheck(AzureIoTHubOptions options)
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
            var transportType = MapToTransportType(_options.ServiceConnectionTransport);
            using (var client = ServiceClient.CreateFromConnectionString(_options.ConnectionString, transportType))
            {
                var result = await client.GetServiceStatisticsAsync(cancellationToken);
                var _ = result.ConnectedDeviceCount;
            }
        }

        private async Task ExecuteRegistryReadCheckAsync()
        {
            using (var client = RegistryManager.CreateFromConnectionString(_options.ConnectionString))
            {
                var query = client.CreateQuery(_options.RegistryReadQuery, 1);
                var _ = await query.GetNextAsJsonAsync();
            }
        }

        /// <summary>
        /// This method try create and remove device for registry write check.
        /// </summary>
        /// <remarks>
        /// In default implementation of configuration <code>deviceId</code> equals <code>"health-check-registry-write-device-id"</code>.
        /// If in previous health check device were not removed, try remove it.
        /// If in previous health check device were added and removed, try create and remove it.
        /// </remarks>
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

        private TransportType MapToTransportType(ServiceConnectionTransport transport)
        {
            switch (transport)
            {
                case ServiceConnectionTransport.Amqp:
                    return TransportType.Amqp;
                case ServiceConnectionTransport.AmqpWebSocketOnly:
                    return TransportType.Amqp_WebSocket_Only;
                default:
                    throw new Exception($"Undefined {nameof(TransportType)}.");
            }
        }
    }
}
