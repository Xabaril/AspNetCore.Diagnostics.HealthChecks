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
                    using (var client = RegistryManager.CreateFromConnectionString(_options.ConnectionString))
                    {
                        var deviceId = _options.RegistryWriteDeviceIdFactory();

                        await client.AddDeviceAsync(new Device(deviceId), cancellationToken);
                        await client.RemoveDeviceAsync(deviceId, cancellationToken);
                    }
                }
                else if (_options.RegistryReadCheck)
                {
                    using (var client = RegistryManager.CreateFromConnectionString(_options.ConnectionString))
                    {
                        var query = client.CreateQuery(_options.RegistryReadQuery, 1);
                        var _ = await query.GetNextAsJsonAsync();
                    }
                }
                if (_options.ServiceConnectionCheck)
                {
                    var transportType = MapToTransportType(_options.ServiceConnectionTransport);
                    using (var client = ServiceClient.CreateFromConnectionString(_options.ConnectionString, transportType))
                    {
                        var result = await client.GetServiceStatisticsAsync(cancellationToken);
                        var _ = result.ConnectedDeviceCount;
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
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
