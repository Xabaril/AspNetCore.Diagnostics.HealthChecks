using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network.Extensions;

namespace HealthChecks.Network
{
    public class PingHealthCheck
        : IHealthCheck
    {
        private readonly PingHealthCheckOptions _options;
        public PingHealthCheck(PingHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var configuredHosts = _options.ConfiguredHosts.Values;

            try
            {
                foreach (var (host, timeout) in configuredHosts)
                {
                    using (var ping = new Ping())
                    {
                        var pingReply = await ping.SendPingAsync(host, timeout);

                        if (pingReply.Status != IPStatus.Success)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Ping check for host {host} is failed with status reply:{pingReply.Status}");
                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
