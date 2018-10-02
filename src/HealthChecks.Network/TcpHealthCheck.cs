using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class TcpHealthCheck 
        : IHealthCheck
    {
        private readonly TcpHealthCheckOptions _options;

        public TcpHealthCheck(TcpHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var (host, port) in _options.ConfiguredHosts)
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(host, port);

                        if (!tcpClient.Connected)
                        {
                            return HealthCheckResult.Failed($"Connection to host {host}:{port} failed");
                        }
                    }
                }

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
