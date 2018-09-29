using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class PingHealthCheck 
        : IHealthCheck
    {
        private readonly PingHealthCheckOptions _options;
        private readonly ILogger<PingHealthCheck> _logger;

        public PingHealthCheck(PingHealthCheckOptions options, ILogger<PingHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var configuredHosts = _options.ConfiguredHosts.Values;

            try
            {
                _logger?.LogInformation($"{nameof(PingHealthCheck)} is checking hosts.");

                foreach (var (host, timeout) in configuredHosts)
                {
                    using (var ping = new Ping())
                    {
                        var pingReply = await ping.SendPingAsync(host, timeout);

                        if (pingReply.Status != IPStatus.Success)
                        {
                            _logger?.LogWarning($"The {nameof(PingHealthCheck)} check failed for host {host} is failed with status reply:{pingReply.Status}.");

                            return HealthCheckResult.Failed($"Ping check for host {host} is failed with status reply:{pingReply.Status}");
                        }
                    }
                }

                _logger?.LogInformation($"The {nameof(PingHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(PingHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
