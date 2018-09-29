using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TcpHealthCheck> _logger;

        public TcpHealthCheck(TcpHealthCheckOptions options, ILogger<TcpHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(TcpHealthCheck)} is checking hosts.");

                foreach (var (host, port) in _options.ConfiguredHosts)
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(host, port);

                        if (!tcpClient.Connected)
                        {
                            _logger?.LogWarning($"The {nameof(TcpHealthCheck)} check failed for host {host} and port {port}.");

                            return HealthCheckResult.Failed($"Connection to host {host}:{port} failed");
                        }
                    }
                }

                _logger?.LogInformation($"The {nameof(TcpHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(TcpHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
