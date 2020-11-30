using HealthChecks.Network.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class DnsResolveHostCountHealthCheck : IHealthCheck
    {
        private readonly DnsResolveCountOptions _options;

        public DnsResolveHostCountHealthCheck(DnsResolveCountOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var resolutionsAboveThreshold = new List<(string host, int total)>();

                foreach (var entry in _options.HostRegistrations)
                {
                    var (minHosts, maxHosts) = entry.Value;

                    var ipAddresses = await Dns.GetHostAddressesAsync(entry.Key).WithCancellationTokenAsync(cancellationToken);
                    var totalAddresses = ipAddresses.Count();

                    if (totalAddresses < minHosts || totalAddresses > maxHosts)
                    {
                        resolutionsAboveThreshold.Add((entry.Key, totalAddresses));
                    }
                }

                if (resolutionsAboveThreshold.Any())
                {
                    var description = string.Join(",", resolutionsAboveThreshold.Select(f => $"Host: {f.host} resolves to {f.total} addresses"));
                    return new HealthCheckResult(context.Registration.FailureStatus, description);
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
