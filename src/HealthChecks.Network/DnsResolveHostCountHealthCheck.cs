using System.Net;
using HealthChecks.Network.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
                List<(string host, int total)>? resolutionsAboveThreshold = null;

                foreach (var entry in _options.HostRegistrations)
                {
                    var (minHosts, maxHosts) = entry.Value;

                    var ipAddresses = await Dns.GetHostAddressesAsync(entry.Key).WithCancellationTokenAsync(cancellationToken);
                    var totalAddresses = ipAddresses.Count();

                    if (totalAddresses < minHosts || totalAddresses > maxHosts)
                    {
                        (resolutionsAboveThreshold ??= new()).Add((entry.Key, totalAddresses));
                    }
                }

                if (resolutionsAboveThreshold?.Count > 0)
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
