using System.Net;
#if !NET5_0_OR_GREATER
using HealthChecks.Network.Extensions;
#endif
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Network;

public class DnsResolveHealthCheck : IHealthCheck
{
    private readonly DnsResolveOptions _options;

    public DnsResolveHealthCheck(DnsResolveOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string>? errorList = null;
            foreach (var item in _options.ConfigureHosts.Values)
            {
#if NET5_0_OR_GREATER
                var ipAddresses = await Dns.GetHostAddressesAsync(item.Host, cancellationToken).ConfigureAwait(false);
#else
                var ipAddresses = await Dns.GetHostAddressesAsync(item.Host).WithCancellationTokenAsync(cancellationToken).ConfigureAwait(false);
#endif

                foreach (var ipAddress in ipAddresses)
                {
                    if (item.Resolutions == null || !item.Resolutions.Contains(ipAddress.ToString()))
                    {
                        (errorList ??= new()).Add($"Ip Address {ipAddress} was not resolved from host {item.Host}");
                        if (!_options.CheckAllHosts)
                        {
                            break;
                        }
                    }
                }
            }

            return errorList.GetHealthState(context);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
