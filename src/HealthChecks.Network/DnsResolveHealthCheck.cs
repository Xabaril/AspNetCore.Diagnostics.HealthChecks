using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class DnsResolveHealthCheck 
        : IHealthCheck
    {
        private readonly DnsResolveOptions _options;
        private readonly ILogger<DnsResolveHealthCheck> _logger;

        public DnsResolveHealthCheck(DnsResolveOptions options, ILogger<DnsResolveHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options)); ;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(DnsResolveHealthCheck)} is checking DNS entries.");

                foreach (var item in _options.ConfigureHosts.Values)
                {
                    var ipAddresses = await Dns.GetHostAddressesAsync(item.Host);

                    foreach (var ipAddress in ipAddresses)
                    {
                        if (!item.Resolutions.Contains(ipAddress.ToString()))
                        {
                            _logger?.LogWarning($"The {nameof(DnsResolveHealthCheck)} check fail for {ipAddress} was not resolved from host {item.Host}.");

                            return HealthCheckResult.Failed("Ip Address {ipAddress} was not resolved from host {item.Host}");
                        }
                    }
                }

                _logger?.LogInformation($"The {nameof(DnsResolveHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(DnsResolveHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
